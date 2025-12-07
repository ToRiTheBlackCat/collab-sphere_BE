using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Evaluate.Commands.StudentEvaluateOtherInTeam
{
    public class StudentEvaluateOtherInTeamHandler : CommandHandler<StudentEvaluateOtherInTeamCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public StudentEvaluateOtherInTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(StudentEvaluateOtherInTeamCommand request, CancellationToken cancellationToken)
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
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam != null && foundTeam.Progress <= 50.0)
                {
                    foreach (var receiver in request.EvaluatorDetails)
                    {
                        //Find existed receiver in team
                        var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, receiver.ReceiverId);
                        if (foundClassMem != null)
                        {
                            foreach (var detail in receiver.ScoreDetails)
                            {
                                //Check if already evaluated
                                var foundEvaluated = await _unitOfWork.MemberEvaluationRepo.SearchEvaluation(request.TeamId, request.RaterId, receiver.ReceiverId, detail.ScoreDetailName);

                                //If already existed
                                if (foundEvaluated != null)
                                {
                                    foundEvaluated.Score = detail.Score;
                                    foundEvaluated.Comment = detail.ScoreDetailName;

                                    _unitOfWork.MemberEvaluationRepo.Update(foundEvaluated);
                                }
                                else
                                {
                                    var newEvaluation = new MemberEvaluation
                                    {
                                        TeamId = foundTeam.TeamId,
                                        RaterId = request.RaterId,
                                        ReceiverId = receiver.ReceiverId,
                                        Score = detail.Score,
                                        Comment = detail.ScoreDetailName,
                                    };

                                    await _unitOfWork.MemberEvaluationRepo.Create(newEvaluation);
                                }

                                await _unitOfWork.SaveChangesAsync();
                            }
                        }
                    }
                }
                else
                {
                    result.Message = "Cannot evaluate and give feedback at this time. Please finish half of the team progress to evaluate and give feedback to other members";
                    return result;
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                return result;
            }

            await _unitOfWork.CommitTransactionAsync();
            result.IsSuccess = true;
            result.Message = "Evaluate and give feedback successfully";
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, StudentEvaluateOtherInTeamCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.STUDENT };

            //Check permission
            if (bypassRoles.Contains(request.RaterRole))
            {
                //Find existed team
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

                //If Student
                if (request.RaterRole == RoleConstants.STUDENT)
                {
                    //Check if rater in the same class and team
                    var foundMemberInClass = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(request.TeamId, request.RaterId);
                    if (foundMemberInClass == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.RaterId),
                            Message = $"You cannot evaluate and give feedback to these students. You are not in the same class or team"
                        });
                        return;
                    }
                }

                //Check if EvaluatorDetails list is empty
                if (!request.EvaluatorDetails.Any() || request.EvaluatorDetails.Count() == 0)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.EvaluatorDetails),
                        Message = $"You cannot evaluate and give feedback to no one. Please choose someone to evaluate"
                    });
                    return;
                }
                else
                {
                    foreach (var receiver in request.EvaluatorDetails)
                    {
                        //Find existed student
                        var foundStudent = await _unitOfWork.UserRepo.GetOneByUserIdAsync(receiver.ReceiverId);
                        if (foundStudent == null)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.EvaluatorDetails),
                                Message = $"Not found any student with ID: {receiver.ReceiverId}"
                            });
                        }
                        else
                        {
                            //Find existed receiver in team
                            var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, receiver.ReceiverId);
                            //Check if receiver is rater
                            if (foundClassMem != null && foundClassMem.StudentId == request.RaterId)
                            {
                                errors.Add(new OperationError
                                {
                                    Field = nameof(request.RaterId),
                                    Message = $"You cannot evaluate and give feedback to your own. Please chooes other student to evaluate and give feedback to."
                                });
                            }
                            else if (foundClassMem == null)
                            {
                                errors.Add(new OperationError
                                {
                                    Field = nameof(request.EvaluatorDetails),
                                    Message = $"You cannot evaluate and give feedback to this student with ID: {receiver.ReceiverId}. Because he/she does not in the same class or not in the same team as you!"
                                });
                            }
                            else
                            {
                                foreach (var detail in receiver.ScoreDetails)
                                {
                                    //Validate Score
                                    if (detail.Score < 0 || detail.Score > 5)
                                    {
                                        errors.Add(new OperationError
                                        {
                                            Field = $"Score detail",
                                            Message = $"You cannot give score < 0 and > 5. Try again with other score"
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                errors.Add(new OperationError()
                {
                    Field = "RaterRole",
                    Message = $"You do not have permission to do this function"
                });
                return;
            }
        }
    }
}
