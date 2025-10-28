using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Evaluate;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass;
using CollabSphere.Application.Features.Team.Queries.GetStudentTeamByAssignClass;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetLecturerEvaluationForTeam
{
    public class GetLecturerEvaluationForTeamHandler : QueryHandler<GetLecturerEvaluationForTeamQuery, GetLecturerEvaluationForTeamResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetLecturerEvaluationForTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetLecturerEvaluationForTeamResult> HandleCommand(GetLecturerEvaluationForTeamQuery request, CancellationToken cancellationToken)
        {
            var result = new GetLecturerEvaluationForTeamResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var foundTeamEva = await _unitOfWork.TeamEvaluationRepo.GetOneByTeamId(request.TeamId);

                if (foundTeamEva != null)
                {
                    var dto = new LecturerEvaluateTeamDto
                    {
                        FinalGrade = foundTeamEva.FinalGrade,
                        TeamComment = foundTeamEva.Comment
                    };

                    //Find the evaluation detail
                    var foundEvaluationDetails = await _unitOfWork.EvaluationDetailRepo.GetEvaluationDetailsByTeamEvaluationId(foundTeamEva.TeamEvaluationId);

                    var detailList = new List<EvaluateDetailForTeam>();
                    foreach (var detail in foundEvaluationDetails)
                    {
                        detailList.Add(new EvaluateDetailForTeam
                        {
                            SubjectGradeComponentId = detail.SubjectGradeComponentId,
                            SubjectGradeComponentName = (await _unitOfWork.SubjectGradeComponentRepo.GetById((int)detail.SubjectGradeComponentId))?.ComponentName,
                            Score = detail.Score,
                            DetailComment = detail.Comment
                        });
                    }

                    dto.EvaluateDetails = detailList;
                    result.LecturerEvaluateTeam = dto;
                    result.Message = $"Get Evaluation and feedback of Lecturer for team with ID: {request.TeamId} successfully";
                    result.IsSuccess = true;
                }
                else
                {
                    result.LecturerEvaluateTeam = null;
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetLecturerEvaluationForTeamQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Check role permission
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
                if (foundTeam == null || foundTeam.Status == 0)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "TeamId",
                        Message = $"Team with the given ID: {request.TeamId} does not exist!"
                    });
                    return;
                }

                //If Lecturer
                if (request.UserRole == RoleConstants.LECTURER)
                {
                    if (request.UserId != foundTeam.LecturerId)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "UserId",
                            Message = $"You are not the lecturer of this team. Cannot view evaluation and feedback of this team"
                        });
                        return;
                    }
                }
                //If Student
                else
                {
                    var classmember = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(foundTeam.ClassId, request.UserId);

                    if (classmember == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"This student with ID: {request.UserId} not in class with ID: {foundTeam.ClassId}. Cannot use this function"
                        });
                        return;
                    }
                    else
                    {
                        //Check if student has team in this class
                        if (classmember.TeamId == null)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"This student with ID: {request.UserId} not has any team in class. Cannot use this function"
                            });
                            return;
                        }
                        else if (classmember.TeamId != foundTeam.TeamId)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"This student with ID: {request.UserId} not in this team. Cannot use this function"
                            });
                            return;
                        }
                    }
                }
            }
        }
    }
}
