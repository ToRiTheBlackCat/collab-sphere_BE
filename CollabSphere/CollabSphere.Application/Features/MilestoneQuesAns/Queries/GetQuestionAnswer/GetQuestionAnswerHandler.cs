using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.MilestoneQuestionAnswers;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQuesAns.Queries.GetQuestionAnswer
{
    public class GetQuestionAnswerHandler : QueryHandler<GetQuestionAnswerQuery, GetQuestionAnswerResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public GetQuestionAnswerHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetQuestionAnswerResult> HandleCommand(GetQuestionAnswerQuery request, CancellationToken cancellationToken)
        {
            var result = new GetQuestionAnswerResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            try
            {
                var foundQuestion = await _unitOfWork.MilestoneQuestionRepo.GetById(request.QuestionId);
                if (foundQuestion != null)
                {
                    var answerDtoList = new List<MilestoneQuestionAnswerDto>();

                    var answerList = await _unitOfWork.MilestoneQuestionAnsRepo.GetAnswersOfQuestionByIdAsync(request.QuestionId);
                    if (answerList.Any() && answerList.Count > 0)
                    {
                        foreach (var ans in answerList)
                        {
                            var answerDto = new MilestoneQuestionAnswerDto
                            {
                                MilestoneQuestionAnsId = ans.MilestoneQuestionAnsId,
                                MilestoneQuestionId = ans.MilestoneQuestionId,
                                ClassMemberId = ans.ClassMemberId,
                                StudentId = -1,
                                StudentName = "",
                                StudentCode = "",
                                StudentAvatar = "",
                                Answer = ans.Answer,
                                CreateTime = ans.CreatedTime,
                            };
                            //Get Student Info
                            var foundClassMem = await _unitOfWork.ClassMemberRepo.GetById(ans.ClassMemberId);
                            var foundStudent = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(foundClassMem.StudentId);

                            //Add Student Info to DTO
                            answerDto.StudentId = foundStudent.UId;
                            answerDto.StudentName = foundStudent.Student.Fullname;
                            answerDto.StudentCode = foundStudent.Student.StudentCode;
                            answerDto.StudentAvatar = await _cloudinaryService.GetImageUrl(foundStudent.Student.AvatarImg);

                            //Get Evaluations of answer
                            var answerEvaluations = await _unitOfWork.AnswerEvaluationRepo.GetAnswerEvaluationsOfAnswer(ans.MilestoneQuestionAnsId);
                            if (answerEvaluations.Any() && answerEvaluations.Count > 0)
                            {
                                var evaluationList = new List<AnswerEvaluationDto>();
                                foreach (var evaluate in answerEvaluations)
                                {
                                    var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(evaluate.EvaluatorId);
                                    var foundEvaluatorAva = await _cloudinaryService.GetImageUrl(foundUser.Student.AvatarImg);
                                    var evaluateDto = new AnswerEvaluationDto
                                    {
                                        AnswerEvaluationId = evaluate.AnswerEvaluationId,
                                        EvaluatorId = evaluate.EvaluatorId,
                                        EvaluatorName = foundUser.Student.Fullname,
                                        EvaluatorCode = foundUser.Student.StudentCode,
                                        EvaluatorAvatar = foundEvaluatorAva,
                                        Score = evaluate.Score,
                                        Comment = evaluate.Comment,
                                        CreateTime = evaluate.CreatedDate,
                                    };

                                    evaluationList.Add(evaluateDto);
                                }

                                //Add list to dto
                                answerDto.AnswerEvaluations = evaluationList;
                            }
                            answerDtoList.Add(answerDto);
                        }
                    }

                    result.AnswersList = answerDtoList;
                    result.IsSuccess = true;
                    result.Message = $"Get answer of question with ID: {request.QuestionId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetQuestionAnswerQuery request)
        {
            //Find Question
            var foundQuestion = await _unitOfWork.MilestoneQuestionRepo.GetById(request.QuestionId);
            if (foundQuestion == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.QuestionId),
                    Message = $"Not found any question with that Id: {request.QuestionId}"
                });
                return;
            }
            else
            {
                //If Lecturer
                if (request.UserRole == RoleConstants.LECTURER)
                {
                    var foundTeam = await _unitOfWork.TeamRepo.GetById(foundQuestion.TeamId);
                    if (foundTeam != null && foundTeam.LecturerId != request.UserId)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"Your are not the lecturer of this class or team. Cannot use this function"
                        });
                        return;
                    }
                }
                //If Student
                else
                {
                    var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundQuestion.TeamId, request.UserId);
                    if (foundClassMem == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"Your are not in this team. Cannot use this function"
                        });
                        return;
                    }
                }
            }
        }
    }
}
