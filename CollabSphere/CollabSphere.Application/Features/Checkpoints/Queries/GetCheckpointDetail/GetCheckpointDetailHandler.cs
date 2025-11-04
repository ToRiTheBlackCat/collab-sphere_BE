using CloudinaryDotNet.Core;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Checkpoints;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Queries.GetCheckpointDetail
{
    public class GetCheckpointDetailHandler : QueryHandler<GetCheckpointDetailQuery, GetCheckpointDetailResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public GetCheckpointDetailHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetCheckpointDetailResult> HandleCommand(GetCheckpointDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new GetCheckpointDetailResult()
            {
                IsSuccess = false,
                IsValidInput = false,
                Message = string.Empty,
            };

            try
            {
                // Check checkpointId
                var checkpoint = await _unitOfWork.CheckpointRepo.GetCheckpointDetail(request.CheckpontId);
                if (checkpoint != null)
                {
                    if (request.UserRole == (int)RoleConstants.LECTURER && checkpoint.TeamMilestone.Team.LecturerId != request.UserId)
                    {
                        result.ErrorList.Add(new OperationError()
                        {
                            Field = nameof(request.UserId),
                            Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID: {checkpoint.TeamMilestone.Team.ClassId}",
                        });
                        return result;
                    }
                    else if (request.UserRole == (int)RoleConstants.STUDENT)
                    {
                        var member = checkpoint.TeamMilestone.Team.ClassMembers
                            .FirstOrDefault(member => member.StudentId == request.UserId);
                        if (member == null)
                        {
                            result.ErrorList.Add(new OperationError()
                            {
                                Field = nameof(request.UserId),
                                Message = $"You ({request.UserId}) are not a member of the team with ID: {checkpoint.TeamMilestone.Team.TeamId}",
                            });
                            return result;
                        }
                    }

                    // Generate Assignment User Image URL
                    foreach (var assignment in checkpoint.CheckpointAssignments)
                    {
                        if (assignment.ClassMember?.Student != null)
                        {
                            var url = await _cloudinaryService.GetImageUrl(assignment.ClassMember.Student.AvatarImg);
                            assignment.ClassMember.Student.AvatarImg = url;
                        }
                    }

                    // Generate Files' User Imgage URL
                    foreach (var checkFile in checkpoint.CheckpointFiles)
                    {
                        if (checkFile.User?.Student != null)
                        {
                            var url = await _cloudinaryService.GetImageUrl(checkFile.User.Student.AvatarImg);
                            checkFile.User.Student.AvatarImg = url;
                        }
                        else if (checkFile.User?.Lecturer != null)
                        {
                            var url = await _cloudinaryService.GetImageUrl(checkFile.User.Lecturer.AvatarImg);
                            checkFile.User.Lecturer.AvatarImg = url;
                        }
                    }

                    result.Checkpoint = checkpoint.ToDetailDto();
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

        protected override async Task ValidateRequest(List<OperationError> errors, GetCheckpointDetailQuery request)
        {
        }
    }
}
