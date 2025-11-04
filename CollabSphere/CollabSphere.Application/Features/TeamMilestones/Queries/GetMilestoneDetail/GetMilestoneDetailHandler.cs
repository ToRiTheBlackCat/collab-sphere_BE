using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.TeamMilestones;
using CollabSphere.Domain.Entities;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestoneDetail
{
    public class GetMilestoneDetailHandler : QueryHandler<GetMilestoneDetailQuery, GetMilestoneDetailResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public GetMilestoneDetailHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetMilestoneDetailResult> HandleCommand(GetMilestoneDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new GetMilestoneDetailResult()
            {
                IsSuccess = false,
                IsValidInput = false,
                Message = string.Empty,
            };

            try
            {
                var milestone = await _unitOfWork.TeamMilestoneRepo.GetDetailsById(request.TeamMilestoneId);
                if (milestone != null)
                {
                    // Checking viewer is Teacher of class/ Member of the team
                    if (request.UserRole == RoleConstants.LECTURER && milestone.Team.Class.LecturerId != request.UserId)
                    {
                        result.ErrorList.Add(new OperationError()
                        {
                            Field = nameof(request.UserId),
                            Message = $"You are not a lecturer of the class with ID:",
                        });
                        return result;
                    }
                    else if (request.UserRole == RoleConstants.STUDENT)
                    {
                        var isTeamMember = milestone.Team.ClassMembers.Select(cm => cm.StudentId).Contains(request.UserId);
                        if (!isTeamMember)
                        {
                            result.ErrorList.Add(new OperationError()
                            {
                                Field = nameof(request.UserId),
                                Message = $"You ({request.UserId}) are not a member of the team with ID: {milestone.Team.TeamId}",
                            });
                            return result;
                        }
                    }

                    // Generate Milestone File's User Avatar Img URL
                    foreach (var file in milestone.MilestoneFiles)
                    {
                        if (file.User?.Lecturer != null)
                        {
                            var url = await _cloudinaryService.GetImageUrl(file.User.Lecturer.AvatarImg);
                            file.User.Lecturer.AvatarImg = url;
                        }
                    }

                    // Generate Milestone Returns' User Avatar Img URL
                    foreach (var file in milestone.MilestoneFiles)
                    {
                        if (file.User?.Student != null)
                        {
                            var url = await _cloudinaryService.GetImageUrl(file.User.Student.AvatarImg);
                            file.User.Student.AvatarImg = url;
                        }
                    }

                    // Generate CheckpointAssignments' Member Avatar Img URL
                    foreach (var assignment in milestone.Checkpoints.SelectMany(x => x.CheckpointAssignments))
                    {
                        if (assignment.ClassMember?.Student != null)
                        {
                            var url = await _cloudinaryService.GetImageUrl(assignment.ClassMember.Student.AvatarImg);
                            assignment.ClassMember.Student.AvatarImg = url;
                        }
                    }

                    // Generate MilestoneEval's Lecturer Avatar Img URL
                    if (milestone.MilestoneEvaluation?.Lecturer != null)
                    {
                        var url = await _cloudinaryService.GetImageUrl(milestone.MilestoneEvaluation.Lecturer.AvatarImg);
                        milestone.MilestoneEvaluation.Lecturer.AvatarImg = url;
                    }

                    result.TeamMilestone = milestone.ToDetailDto();
                }

                result.IsValidInput = true;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetMilestoneDetailQuery request)
        {
            return;
        }
    }
}
