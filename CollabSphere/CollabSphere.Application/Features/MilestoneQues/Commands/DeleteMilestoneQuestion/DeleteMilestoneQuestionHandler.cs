using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQues.Commands.DeleteMilestoneQuestion
{
    public class DeleteMilestoneQuestionHandler : CommandHandler<DeleteMilestoneQuestionCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteMilestoneQuestionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteMilestoneQuestionCommand request, CancellationToken cancellationToken)
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

                var foundMilestoneQues = await _unitOfWork.MilestoneQuestionRepo.GetById(request.QuestionId);
                if (foundMilestoneQues != null)
                {
                    _unitOfWork.MilestoneQuestionRepo.Delete(foundMilestoneQues);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Delete Milestone question with ID: {request.QuestionId} successfully";
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.ToString();
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteMilestoneQuestionCommand request)
        {
            //Find existed milestone question
            var foundQues = await _unitOfWork.MilestoneQuestionRepo.GetById(request.QuestionId);
            if (foundQues == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.QuestionId),
                    Message = $"Cannot find any milestone question with ID: {request.QuestionId}"
                });
                return;
            }
            else
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetById(foundQues.TeamId);

                if (foundTeam != null && foundTeam.LecturerId != request.UserId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserId),
                        Message = $"You are not the lecturer of this team with ID: {request.UserId}. Cannot use this function"
                    });
                    return;
                }

                //Check answer of milestone question
                var foundAnswers = await _unitOfWork.MilestoneQuestionAnsRepo.GetAnswersOfQuestionByIdAsync(request.QuestionId);
                if(foundAnswers != null && ( foundAnswers.Any() || foundAnswers.Count() > 0))
                {
                    errors.Add(new OperationError
                    {
                        Field = "Question Answer",
                        Message = $"This question with ID: {request.QuestionId} already have answers. Cannot delete this question"
                    });
                    return;
                }
            }
        }
    }
}
