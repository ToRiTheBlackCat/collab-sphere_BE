using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQues.Commands
{
    public class CreateMilestoneQuestionHandler : CommandHandler<CreateMilestoneQuestionCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateMilestoneQuestionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateMilestoneQuestionCommand request, CancellationToken cancellationToken)
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

                var newMilestoneQues = new MilestoneQuestion
                {
                    TeamMilestoneId = request.TeamMilestoneId,
                    TeamId = request.TeamId,
                    Question = request.Question.Trim(),
                    AnswerCount = 0,
                    CreatedTime = DateTime.UtcNow
                };

                await _unitOfWork.MilestoneQuestionRepo.Create(newMilestoneQues);
                await _unitOfWork.SaveChangesAsync();
                result.IsSuccess = true;
                result.Message = $"Create Milestone Question for team with ID: {request.TeamId} and team milestone with ID: {request.TeamMilestoneId} successfully";
             
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            await _unitOfWork.CommitTransactionAsync();
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateMilestoneQuestionCommand request)
        {
            //Find existed team milestone
            var foundTeamMilestone = await _unitOfWork.TeamMilestoneRepo.GetTeamMilestoneById(request.TeamMilestoneId);
            if (foundTeamMilestone == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Cannot find any team milestone with ID: {request.TeamMilestoneId}"
                });
                return;
            }

            //Find existed Team
            var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
            if (foundTeam == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.TeamId),
                    Message = $"Cannot find any team with ID: {request.TeamId}"
                });
                return;
            }
            else
            {
                if(foundTeamMilestone.TeamId != foundTeam.TeamId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.TeamId),
                        Message = $"This milestone is not belong to team with ID: {request.TeamId}"
                    });
                    return;
                }

                //Check if lecturer of team
                if (foundTeam.LecturerId != request.UserId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserId),
                        Message = $"You are not the lecturer of this team with ID: {request.TeamId}. Cannot use this function"
                    });
                    return;
                }
            }
        }
    }
}
