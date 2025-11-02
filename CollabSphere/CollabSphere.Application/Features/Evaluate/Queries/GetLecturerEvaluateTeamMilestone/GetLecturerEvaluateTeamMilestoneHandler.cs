using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Evaluate;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Evaluate.Queries.GetLecturerEvaluationForTeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetLecturerEvaluateTeamMilestone
{
    public class GetLecturerEvaluateTeamMilestoneHandler : QueryHandler<GetLecturerEvaluateTeamMilestoneQuery, GetLecturerEvaluateTeamMilestoneResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;
        public GetLecturerEvaluateTeamMilestoneHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetLecturerEvaluateTeamMilestoneResult> HandleCommand(GetLecturerEvaluateTeamMilestoneQuery request, CancellationToken cancellationToken)
        {
            var result = new GetLecturerEvaluateTeamMilestoneResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            try
            {
                var foundTeamMilestone = await _unitOfWork.TeamMilestoneRepo.GetTeamMilestoneById(request.TeamMilestoneId);
                if (foundTeamMilestone != null)
                {
                    var foundTeam = await _unitOfWork.TeamRepo.GetById(foundTeamMilestone.TeamId);
                    if (foundTeam != null)
                    {
                        var foundMileEvaluation = await _unitOfWork.MilestoneEvaluationRepo.GetEvaluationOfMilestone(request.TeamMilestoneId, foundTeam.LecturerId, foundTeam.TeamId);

                        if (foundMileEvaluation != null)
                        {
                            var foundLecturer = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(foundTeam.LecturerId);

                            //Create response DTO
                            var dto = new GetLecturerEvaluateTeamMilestoneDto
                            {
                                LecturerId = foundTeam.LecturerId,
                                LecturerName = foundTeam.LecturerName,
                                AvatarUrl = (await _cloudinaryService.GetImageUrl(foundLecturer.Lecturer.AvatarImg)),
                                Score = foundMileEvaluation.Score,
                                Comments = foundMileEvaluation.Comment,
                                CreatedDate = DateTime.UtcNow
                            };

                            result.LecturerEvaluateTeamMilestone = dto;
                        }
                    }

                    result.IsSuccess = true;
                    result.Message = $"Get lecturer evaluate team milestone with ID: {request.TeamMilestoneId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetLecturerEvaluateTeamMilestoneQuery request)
        {
            //Find team milestone 
            var foundTeamMilestone = await _unitOfWork.TeamMilestoneRepo.GetTeamMilestoneById(request.TeamMilestoneId);
            if (foundTeamMilestone == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.UserId),
                    Message = $"Cannot find any team milestone with ID: {request.TeamMilestoneId}"
                });
                return;
            }
            else
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetById(foundTeamMilestone.TeamId);
                if (foundTeam != null)
                {
                    //If Lecturer
                    if (request.UserRole == RoleConstants.LECTURER)
                    {
                        if (foundTeam.LecturerId != request.UserId)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"You are not the lecturer of this team. Cannot use this function"
                            });
                            return;
                        }
                    }
                    else
                    {
                        var foundTeamMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, request.UserId);
                        if (foundTeamMem == null)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"You are not the member of this team. Cannot use this function"
                            });
                            return;
                        }
                    }
                }
            }
        }
    }
}
