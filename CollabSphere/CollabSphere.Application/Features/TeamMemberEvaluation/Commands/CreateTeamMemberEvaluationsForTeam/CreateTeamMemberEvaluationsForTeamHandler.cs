using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMemberEvaluation.Commands.CreateTeamMemberEvaluationsForTeam
{
    public class CreateTeamMemberEvaluationsForTeamHandler : CommandHandler<CreateTeamMemberEvaluationsForTeamCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private Domain.Entities.Team _foundTeam;
        public CreateTeamMemberEvaluationsForTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateTeamMemberEvaluationsForTeamCommand request, CancellationToken cancellationToken)
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

                foreach (var memberScore in request.MemberScores)
                {
                    //Check if already grade
                    var existingEvaluation = await _unitOfWork.TeamMemEvaluationRepo
                        .GetTeamMemEvaluations(request.TeamId, null, memberScore.ClassMemberId);
                    if (existingEvaluation != null && existingEvaluation.Count != 0)
                    {
                        var updateEvaluation = existingEvaluation.First();
                        updateEvaluation.Score = memberScore.Score;
                        _unitOfWork.TeamMemEvaluationRepo.Update(updateEvaluation);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    else
                    {
                        //Create new
                        var newMemberEvaluation = new Domain.Entities.TeamMemEvaluation()
                        {
                            TeamId = request.TeamId,
                            LecturerId = request.UserId,
                            ClassMemberId = memberScore.ClassMemberId,
                            Score = memberScore.Score,
                        };
                        await _unitOfWork.TeamMemEvaluationRepo.Create(newMemberEvaluation);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
                result.IsSuccess = true;
                result.Message = "Grade scores for members in this team successfully";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Fail to grade score for member in this team";
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateTeamMemberEvaluationsForTeamCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER };
            if (!bypassRoles.Contains(request.UserRole))
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"You not have permission to do this function"
                });
                return;
            }
            else
            {
                //Check if team exists
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null || foundTeam.Status == (int)TeamStatus.DEACTIVE)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "TeamId",
                        Message = $"Team with the given ID: {request.TeamId} does not exist!"
                    });
                    return;
                }
                else
                {
                    _foundTeam = foundTeam;
                }

                var foundLecturer = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId);
                if (foundLecturer == null || !foundLecturer.IsActive)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "UserId",
                        Message = $"Lecturer with the given ID: {request.UserId} does not exist!"
                    });
                    return;
                }
                else
                {
                    //Check if requester is the lecturer of this team
                    if (request.UserId != foundTeam.LecturerId)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "UserId",
                            Message = $"You are not the lecturer of this team. Cannot give score for member of this team"
                        });
                        return;
                    }
                }

                //Check if the request.Members is valid member of this team
                var validMemberIds = (await _unitOfWork.ClassMemberRepo
                    .GetClassMemberAsyncByTeamId(request.TeamId))?
                    .Select(x => x.ClassMemberId)
                    .ToHashSet();

                var invalidMemberIds = request.MemberScores
                    .Select(me => me.ClassMemberId)
                    .Where(id => !validMemberIds.Contains(id))
                    .ToList();

                if (invalidMemberIds.Any())
                {
                    foreach (var invalidId in invalidMemberIds)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "ClassMemberId",
                            Message = $"ClassMemberId with ID: {invalidId} is not a valid member of Team ID: {request.TeamId}. Cannot grade this member"
                        });
                    }
                }

                // Validate score values (0–10)
                foreach (var memberScore in request.MemberScores)
                {
                    if (memberScore.Score != null && (memberScore.Score < 0 || memberScore.Score > 10))
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "Score",
                            Message = $"Invalid score. Score for ClassMemberId {memberScore.ClassMemberId} must be between 0 and 10"
                        });
                    }
                }
            }
        }
    }
}
