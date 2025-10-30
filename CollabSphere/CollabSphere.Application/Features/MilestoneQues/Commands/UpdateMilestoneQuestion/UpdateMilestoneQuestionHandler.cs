using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQues.Commands.UpdateMilestoneQuestion
{
    public class UpdateMilestoneQuestionHandler : CommandHandler<UpdateMilestoneQuestionCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateMilestoneQuestionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateMilestoneQuestionCommand request, CancellationToken cancellationToken)
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

                var foundMileQues = await _unitOfWork.MilestoneQuestionRepo.GetById(request.QuestionId);
                if (foundMileQues != null)
                {
                    foundMileQues.Question = request.Question.Trim();
                    foundMileQues.CreatedTime = DateTime.UtcNow;
                    _unitOfWork.MilestoneQuestionRepo.Update(foundMileQues);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Update milestone question with ID: {request.QuestionId} successfully";
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateMilestoneQuestionCommand request)
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
            }
        }
    }
}
