using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Checkpoints.Commands.AssignMembersToCheckpoint
{
    public class AssignMembersToCheckpointHandler : CommandHandler<AssignMembersToCheckpointCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IConfiguration _configure;
        private readonly EmailSender _emailSender;

        public AssignMembersToCheckpointHandler(IUnitOfWork unitOfWork, IConfiguration configure)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _emailSender = new EmailSender(_configure);
        }

        protected override async Task<CommandResult> HandleCommand(AssignMembersToCheckpointCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Get assignments of checkpoint
                var currentAssignments = await _unitOfWork.CheckpointAssignmentRepo.GetByCheckpointId(request.AssignmentsDto.CheckpointId);
                var currentMemberIds = currentAssignments.Select(x => x.ClassMemberId).ToHashSet();

                // Remove assignments that are not in request
                var removeCount = 0;
                foreach (var assignment in currentAssignments)
                {
                    if (request.AssignmentsDto.ClassMemberIds.Contains(assignment.ClassMemberId))
                    {
                        continue;
                    }

                    _unitOfWork.CheckpointAssignmentRepo.Delete(assignment);
                    removeCount++;
                }
                await _unitOfWork.SaveChangesAsync();
                //Create receiver email list
                var receiverEmails = new HashSet<string>();

                // Create new assignments
                var newMemberIds = request.AssignmentsDto.ClassMemberIds
                    .Except(currentMemberIds);
                var assignDate = DateTime.UtcNow;
                foreach (var classMemberId in newMemberIds)
                {
                    var newAssignment = new CheckpointAssignment()
                    {
                        CheckpointId = request.AssignmentsDto.CheckpointId,
                        ClassMemberId = classMemberId,
                        AssignedDate = assignDate,
                    };
                    await _unitOfWork.CheckpointAssignmentRepo.Create(newAssignment);

                    //Found user to send mail
                    var foundClassMember = await _unitOfWork.ClassMemberRepo.GetById(classMemberId);
                    var foundStu = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(foundClassMember!.StudentId);

                    //Add to receiverEmails 
                    receiverEmails.Add(foundStu.Email);
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();
                //Send Email
                var foundCheckpoint = await _unitOfWork.CheckpointRepo.GetById(request.AssignmentsDto.CheckpointId);
                await _emailSender.SendNotiEmailsForCheckpoint(receiverEmails, foundCheckpoint);

                result.Message = $"Updated member assignments for checkpoint with ID '{request.AssignmentsDto.CheckpointId}'. " +
                    $"Added {newMemberIds.Count()} member(s), Removed {removeCount} member(s).";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, AssignMembersToCheckpointCommand request)
        {
            var dto = request.AssignmentsDto;

            // Check checkpointId
            var checkpoint = await _unitOfWork.CheckpointRepo.GetCheckpointDetail(dto.CheckpointId);
            if (checkpoint == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.CheckpointId),
                    Message = $"No checkpoint with ID: {dto.CheckpointId}",
                });
                return;
            }

            // Check if user is team member
            if (request.UserRole == RoleConstants.STUDENT)
            {
                var member = checkpoint.TeamMilestone.Team.ClassMembers
                    .FirstOrDefault(x => x.StudentId == request.UserId);
                if (member == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID: {checkpoint.TeamMilestone.Team.TeamId}"
                    });
                    return;
                }
            }

            // Check if assigned members are members in team
            var validMemberIds = checkpoint.TeamMilestone.Team.ClassMembers
                .Select(x => x.ClassMemberId);
            var invalidMemberIds = dto.ClassMemberIds.Except(validMemberIds);
            if (invalidMemberIds.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.ClassMemberIds),
                    Message = $"The ClassMemberIds '{string.Join(", ", invalidMemberIds)}' are not members of the team with ID '{checkpoint.TeamMilestone.Team.TeamId}'" +
                        $". Valid ClassMemberIds for the team are: {string.Join(", ", validMemberIds)}"
                });
                return;
            }
        }
    }
}
