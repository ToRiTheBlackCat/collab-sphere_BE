using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Evaluate.Commands.EvaluateMileQuestionAns
{
    public class EvaluateMilestoneQuestionAnswerHandler : CommandHandler<EvaluateMilestoneQuestionAnswerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public EvaluateMilestoneQuestionAnswerHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(EvaluateMilestoneQuestionAnswerCommand request, CancellationToken cancellationToken)
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

                var foundAns = await _unitOfWork.MilestoneQuestionAnsRepo.GetAnswerById(request.AnswerId);

                if (foundAns != null)
                {
                    var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundAns.TeamId, request.EvaluatorId);

                    var newAnsEvaluation = new AnswerEvaluation
                    {
                        MilestoneQuestionAnsId = foundAns.MilestoneQuestionAnsId,
                        TeamId = foundAns.TeamId,
                        EvaluatorId = request.EvaluatorId,
                        ReceiverId = foundClassMem.StudentId,
                        Score = request.Score,
                        Comment = request.Comment,
                        CreatedDate = DateTime.UtcNow,
                    };

                    await _unitOfWork.AnswerEvaluationRepo.Create(newAnsEvaluation);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Evaluate answer successfully";
                }

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                return result;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, EvaluateMilestoneQuestionAnswerCommand request)
        {
            //Find Answer
            var foundAns = await _unitOfWork.MilestoneQuestionAnsRepo.GetAnswerById(request.AnswerId);
            if (foundAns == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.AnswerId),
                    Message = $"Cannot find any answer with ID: {request.AnswerId}"
                });
                return;
            }
            else
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetById(foundAns.TeamId);
                if (foundTeam != null)
                {
                    var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, request.EvaluatorId);
                    if (foundClassMem == null)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.EvaluatorId),
                            Message = "You are not the student of this team. Cannot use this function"
                        });
                        return;
                    }

                }
            }

        }
    }
}
