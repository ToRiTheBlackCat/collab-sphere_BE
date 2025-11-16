using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Commands.CreateTeam;
using CollabSphere.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeam
{
    public class LecturerEvaluateTeamHandler : CommandHandler<LecturerEvaluateTeamCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public LecturerEvaluateTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(LecturerEvaluateTeamCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            decimal teamScore = 0;

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                //Find existed team evaluate
                var foundTeamEvaluate = await _unitOfWork.TeamEvaluationRepo.GetOneByTeamId(request.TeamId);
                if (foundTeamEvaluate != null)
                {
                    result.Message = "Already evaluate and feedback this team. Cannot evaluate more";

                    return result;
                }

                var newTeamEvaluate = new TeamEvaluation
                {
                    TeamId = request.TeamId,
                    LecturerId = request.UserId,
                    Comment = request.TeamComment
                };
                await _unitOfWork.TeamEvaluationRepo.Create(newTeamEvaluate);
                await _unitOfWork.SaveChangesAsync();

                foreach (var detail in request.EvaluateDetails)
                {
                    //Find gradeComponent
                    var foundGradeComponent = await _unitOfWork.SubjectGradeComponentRepo.GetById(detail.SubjectGradeComponentId);
                    if (foundGradeComponent == null)
                    {
                        continue;
                    }

                    //Create detail
                    var evaluateDetail = new EvaluationDetail
                    {
                        TeamEvaluationId = newTeamEvaluate.TeamEvaluationId,
                        SubjectGradeComponentId = detail.SubjectGradeComponentId,
                        Percentage = foundGradeComponent.ReferencePercentage ?? 0,
                        Score = detail.Score,
                        Comment = detail.DetailComment,
                    };
                    await _unitOfWork.EvaluationDetailRepo.Create(evaluateDetail);
                    await _unitOfWork.SaveChangesAsync();

                    //Calculate final score
                    teamScore += evaluateDetail.Score * (evaluateDetail.Percentage / 100);
                }

                newTeamEvaluate.FinalGrade = teamScore;
                _unitOfWork.TeamEvaluationRepo.Update(newTeamEvaluate);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                return result;
            }

            await _unitOfWork.CommitTransactionAsync();
            result.IsSuccess = true;
            result.Message = $"Successfully evaluate and give feedback for team with Id: {request.TeamId}";

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, LecturerEvaluateTeamCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER };

            if (bypassRoles.Contains(request.UserRole))
            {
                //Validate empty EvaluateDetail list
                if (!request.EvaluateDetails.Any() || request.EvaluateDetails.Count() == 0)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.EvaluateDetails),
                        Message = $"Cannot evaluate with empty list of evaluate details"
                    });
                    return;
                }

                //Validate teamId
                var foundTeam = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
                if (foundTeam == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.TeamId),
                        Message = $"Not found any team with that Id: {request.TeamId}"
                    });
                    return;
                }

                //Validate lecturerId
                var foundLecturer = await _unitOfWork.UserRepo.GetOneByUserIdAsync(request.UserId);
                if (foundLecturer == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserId),
                        Message = $"Not found any Lecturer with that Id: {request.UserId}"
                    });
                    return;
                }
                else
                {
                    //Find project
                    var foundProject = await _unitOfWork.ProjectRepo.GetById(foundTeam.ProjectAssignment.ProjectId);
                    var gradeComponentIdSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    if (foundProject != null)
                    {
                        //Find subject syllabus 
                        var foundSyllabus = await _unitOfWork.SubjectRepo.GetById(foundProject.SubjectId);

                        if(foundSyllabus != null)
                        {
                            //Find grade component list of that syllabus
                            var gradeComponents = await _unitOfWork.SubjectGradeComponentRepo.GetComponentsBySubjectId(foundSyllabus.SubjectId);

                            foreach (var component in gradeComponents)
                            {
                                gradeComponentIdSet.Add(component.SubjectGradeComponentId.ToString());
                            }
                        }
                        
                    }
                    //Validate permission to evaluate team
                    if (request.UserId != foundTeam.LecturerId)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"You are not the lecturer of this class. Not has permission to evaluate this team"
                        });
                        return;
                    }

                    foreach (var detail in request.EvaluateDetails)
                    {
                        //  Check if gradecomponentId exists in the syllabus grade component set
                        if (!gradeComponentIdSet.Contains(detail.SubjectGradeComponentId.ToString()))
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(detail.SubjectGradeComponentId),
                                Message = $"Subject grade component with Id: {detail.SubjectGradeComponentId} does not belong to this subject syllabus."
                            });
                        }

                        //Validate score
                        if (detail.Score > 10)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(detail.Score),
                                Message = $"Cannot give score more than 10 score"
                            });
                        }
                    }
                }
            }
            else
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"You do not have permission to do this function"
                });
                return;
            }
        }
    }
}
