using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQuesAns.Commands.UpdateQuestionAnswer
{
    public class UpdateQuestionAnswerHandler : CommandHandler<UpdateQuestionAnswerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateQuestionAnswerHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(UpdateQuestionAnswerCommand request, CancellationToken cancellationToken)
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

                var foundAnswer = await _unitOfWork.MilestoneQuestionAnsRepo.GetAnswerById(request.AnswerId);
                if (foundAnswer != null)
                {
                    foundAnswer.Answer = request.Answer.Trim();
                    foundAnswer.CreatedTime = DateTime.UtcNow;

                    _unitOfWork.MilestoneQuestionAnsRepo.Update(foundAnswer);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Update question answer with ID: {request.AnswerId} successfully";
                }

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateQuestionAnswerCommand request)
        {
            //Find existed milestone question
            var foundMileQues = await _unitOfWork.MilestoneQuestionRepo.GetById(request.QuestionId);
            if (foundMileQues == null)
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
                //Fing existed answer
                var foundAns = await _unitOfWork.MilestoneQuestionAnsRepo.GetById(request.AnswerId);
                if (foundAns == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.AnswerId),
                        Message = $"Cannot find any milestone question answer with ID: {request.AnswerId}"
                    });
                    return;
                }

                //Fing classMem
                var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundMileQues.TeamId, request.UserId);
                if (foundClassMem == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserId),
                        Message = $"You are not in this class or team. Cannot use this function"
                    });
                    return;
                }

                if (foundAns.ClassMemberId != foundClassMem.ClassMemberId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserId),
                        Message = $"You are not the owner of this answer. Cannot update this answer"
                    });
                    return;
                }
            }
        }
    }
}
