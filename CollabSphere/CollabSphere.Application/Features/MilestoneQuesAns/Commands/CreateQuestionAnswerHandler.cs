using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQuesAns.Commands
{
    public class CreateQuestionAnswerHandler : CommandHandler<CreateQuestionAnswerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateQuestionAnswerHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateQuestionAnswerCommand request, CancellationToken cancellationToken)
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
                    var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundMileQues.TeamId, request.UserId);

                    var newAns = new Domain.Entities.MilestoneQuestionAn
                    {
                        MilestoneQuestionId = request.QuestionId,
                        TeamId = foundMileQues.TeamId,
                        ClassMemberId = foundClassMem.ClassMemberId,
                        Answer = request.Answer,
                        CreatedTime = DateTime.UtcNow,
                    };

                    await _unitOfWork.MilestoneQuestionAnsRepo.Create(newAns);
                    await _unitOfWork.SaveChangesAsync();

                    //Update answer count of question
                    foundMileQues.AnswerCount++;
                    _unitOfWork.MilestoneQuestionRepo.Update(foundMileQues);
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = "Answer milestone question successfully";
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateQuestionAnswerCommand request)
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
                //Check if requester in the same team
                var foundTeamMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundMileQues.TeamId, request.UserId);

                if (foundTeamMem == null ||
                    (foundTeamMem != null && foundTeamMem.TeamId != foundMileQues.TeamId))
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserId),
                        Message = $"You are not this class or team. Cannot use this function"
                    });
                }
            }
        }
    }
}
