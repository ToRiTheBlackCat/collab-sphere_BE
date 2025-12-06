using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands.UpdateTeam
{
    public class UpdateTeamHandler : CommandHandler<UpdateTeamCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTeamHandler> _logger;

        public UpdateTeamHandler(IUnitOfWork unitOfWork,
                                 ILogger<UpdateTeamHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateTeamCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find existed team
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null)
                {
                    result.Message = $"Cannot find any team with that Id: {request.TeamId}";

                    return result;
                }

                //Update team
                foundTeam.TeamName = request.TeamName.Trim();
                //Enrol Key
                foundTeam.EnrolKey = string.IsNullOrWhiteSpace(request.EnrolKey)
                    ? foundTeam.EnrolKey
                    : request.EnrolKey.Trim();
                //Description
                foundTeam.Description = string.IsNullOrWhiteSpace(request.Description)
                    ? foundTeam.Description
                    : request.Description.Trim();
                //Git Link
                foundTeam.GitLink = string.IsNullOrWhiteSpace(request.GitLink)
                    ? foundTeam.GitLink
                    : request.GitLink.Trim();

                _unitOfWork.TeamRepo.Update(foundTeam);
                await _unitOfWork.SaveChangesAsync();

                #region Update Team Members
                #region Update Leader 
                if (request.LeaderId != foundTeam.LeaderId)
                {
                    //Change old leader
                    var foundOldLeader = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(foundTeam.ClassId, foundTeam.LeaderId);

                    if (foundOldLeader != null)
                    {
                        //Remove all info of leader
                        foundOldLeader.TeamId = null;
                        foundOldLeader.TeamRole = null;
                        foundOldLeader.IsGrouped = false;
                        _unitOfWork.ClassMemberRepo.Update(foundOldLeader);
                        await _unitOfWork.SaveChangesAsync();

                        //Update new leader to team
                        foundTeam.LeaderId = request.LeaderId;
                        _unitOfWork.TeamRepo.Update(foundTeam);
                        await _unitOfWork.SaveChangesAsync();

                        //Update status of class member
                        var foundClassMember = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(foundTeam.ClassId, request.LeaderId);
                        if (foundClassMember != null)
                        {
                            foundClassMember.TeamId = foundTeam.TeamId;
                            foundClassMember.TeamRole = (int?)TeamRole.LEADER;
                            foundClassMember.IsGrouped = true;

                            _unitOfWork.ClassMemberRepo.Update(foundClassMember);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }
                #endregion

                #region Update other members
                var incomingStudentIds = request.StudentList
                    .Select(s => s.StudentId)
                    .ToHashSet();
                //Get current team members
                var currentTeamMembers = await _unitOfWork.ClassMemberRepo
                    .GetClassMemberAsyncByTeamId(foundTeam.TeamId);
                //Get members prepare to remove (except leader)
                var membersToRemove = currentTeamMembers
                    .Where(m => !incomingStudentIds.Contains(m.StudentId) && m.StudentId != request.LeaderId)
                    .ToList();
                //Remove members who are not in the incoming list
                if (membersToRemove.Any())
                {
                    foreach (var member in membersToRemove)
                    {
                        member.TeamId = null;
                        member.TeamRole = null;
                        member.IsGrouped = false;
                        _unitOfWork.ClassMemberRepo.Update(member);
                    }
                }

                //Add incoming new members
                var currentMemberIds = currentTeamMembers.Select(m => m.StudentId).ToHashSet();
                //Get members prepare to add
                var newStudentIdsToAdd = incomingStudentIds
                    .Where(id => !currentMemberIds.Contains(id))
                    .ToList();

                if (newStudentIdsToAdd.Any())
                {
                    // Get all ClassMembers of class
                    var members = await _unitOfWork.ClassMemberRepo
                        .GetByClassIdAndStudentIdsAsync(foundTeam.ClassId, newStudentIdsToAdd);

                    foreach (var mem in members)
                    {
                        // Check if that member is already in another team
                        if (mem.IsGrouped && mem.TeamId != request.TeamId)
                        {
                            throw new Exception($"Student with ID: {mem.StudentId} is already in another team.");
                        }

                        //Not in any team
                        mem.TeamId = foundTeam.TeamId;
                        mem.TeamRole = (int)TeamRole.MEMBER;
                        mem.IsGrouped = true;

                        _unitOfWork.ClassMemberRepo.Update(mem);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = $"Team '{foundTeam.TeamName}' updated successfully.";
                _logger.LogInformation("Team updated successfully with ID: {TeamId}", request.TeamId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                _logger.LogError(ex, "Error occurred while updating team.");
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateTeamCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Check team exists
            var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
            if (foundTeam == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.TeamId),
                    Message = $"Not found any team with that Id: {request.TeamId}"
                });
                return;
            }

            //Check if role is valid to delete team
            if (bypassRoles.Contains(request.UserRole))
            {
                //If Lecturer
                if (request.UserRole == RoleConstants.LECTURER)
                {
                    //Check if lecturer exists
                    var foundLecturer = await _unitOfWork.LecturerRepo.GetById(request.UserId);
                    if (foundLecturer == null)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "UserId",
                            Message = $"Lecturer with the given ID: {request.UserId} does not exist."
                        });
                    }

                    //Check if lecturer is the owner of the team
                    if (request.UserId != foundTeam.LecturerId)
                        errors.Add(new OperationError()
                        {
                            Field = "UserRole",
                            Message = $"This lecturer with ID: {request.UserId} not has permission to update this team."
                        });
                    return;
                }

                //IF Student
                if (request.UserRole == RoleConstants.STUDENT)
                {
                    //Check if student exists
                    var foundStudent = await _unitOfWork.StudentRepo.GetById(request.UserId);
                    if (foundStudent == null)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "UserId",
                            Message = $"Student with the given ID: {request.UserId} does not exist."
                        });
                    }

                    //Check if student is the leader of the team
                    if (request.UserId != foundTeam.LeaderId)
                        errors.Add(new OperationError()
                        {
                            Field = "UserRole",
                            Message = $"This student with ID: {request.UserId} is not the leader - not has permission to update this team."
                        });
                    return;
                }
            }
        }
    }
}
