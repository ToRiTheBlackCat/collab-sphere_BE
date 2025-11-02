using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeamMilestone
{
    public class LecturerEvaluateTeamMilestoneHandler : CommandHandler<LecturerEvaluateTeamMilestoneCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public LecturerEvaluateTeamMilestoneHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(LecturerEvaluateTeamMilestoneCommand request, CancellationToken cancellationToken)
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
                var foundTeamMilestone = await _unitOfWork.TeamMilestoneRepo.GetTeamMilestoneById(request.TeamMilestoneId);
                if (foundTeamMilestone != null)
                {
                    var foundTeam = await _unitOfWork.TeamRepo.GetById(foundTeamMilestone.TeamId);
                    if (foundTeam != null)
                    {
                        //Check existed milestone evaluation
                        var foundMileEvaluation = await _unitOfWork.MilestoneEvaluationRepo.GetEvaluationOfMilestone(request.TeamMilestoneId, request.EvaluatorId, foundTeam.TeamId);

                        //If existed - updated
                        if (foundMileEvaluation != null)
                        {
                            foundMileEvaluation.Score = request.Score;
                            foundMileEvaluation.Comment = request.Comment;
                            foundMileEvaluation.CreatedDate = DateTime.UtcNow;

                            _unitOfWork.MilestoneEvaluationRepo.Update(foundMileEvaluation);
                            await _unitOfWork.SaveChangesAsync();

                            result.Message = $"Update milestone evaluation for team milestone with ID: {foundMileEvaluation.MilestoneId} successfully";
                        }
                        else
                        {
                            //Create new evaluation
                            var newMileEvaluation = new MilestoneEvaluation
                            {
                                MilestoneId = request.TeamMilestoneId,
                                LecturerId = request.EvaluatorId,
                                TeamId = foundTeam.TeamId,
                                Score = request.Score,
                                Comment = request.Comment,
                                CreatedDate = DateTime.UtcNow
                            };
                            await _unitOfWork.MilestoneEvaluationRepo.Create(newMileEvaluation);
                            await _unitOfWork.SaveChangesAsync();

                            result.Message = $"Evaluate for team milestone with ID: {request.TeamMilestoneId} successfully";
                        }

                    }

                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, LecturerEvaluateTeamMilestoneCommand request)
        {
            //Find team milestone
            var foundTeamMilestone = await _unitOfWork.TeamMilestoneRepo.GetTeamMilestoneById(request.TeamMilestoneId);
            if (foundTeamMilestone == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Not found any Team Milestone with that Id: {request.TeamMilestoneId}"
                });
                return;
            }
            else
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetById(foundTeamMilestone.TeamId);
                if (foundTeam != null)
                {
                    //Check if lecturer of team
                    if (foundTeam.LecturerId != request.EvaluatorId)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.EvaluatorId),
                            Message = $"You are not the lecturer of this team. Cannot use this function"
                        });
                        return;
                    }
                }
            }
        }
    }
}
