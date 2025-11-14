using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.TeamFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamFiles.Queries.GetTeamFiles
{
    public class GetTeamFilesHandler : QueryHandler<GetTeamFilesQuery, GetTeamFilesResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public GetTeamFilesHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetTeamFilesResult> HandleCommand(GetTeamFilesQuery request, CancellationToken cancellationToken)
        {
            var result = new GetTeamFilesResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                // Get team files
                var teamFiles = await _unitOfWork.TeamFileRepo.GetFilesByTeam(request.TeamId);

                // Generate URL for avatar img
                foreach (var teamFile in teamFiles)
                {
                    if (teamFile.User?.IsTeacher == true && teamFile.User.Lecturer != null)
                    {
                        teamFile.User.Lecturer.AvatarImg = await _cloudinaryService.GetImageUrl(teamFile.User.Lecturer.AvatarImg);
                    }
                    else if (teamFile.User?.IsTeacher == false && teamFile.User.Student != null)
                    {
                        teamFile.User.Student.AvatarImg = await _cloudinaryService.GetImageUrl(teamFile.User.Student.AvatarImg);
                    }
                }

                result.Grouping = teamFiles.ToViewModel()
                    .GroupBy(x => x.FilePathPrefix)
                    .ToDictionary(x => x.Key);
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetTeamFilesQuery request)
        {
            // Check TeamId
            var team = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
            if (team == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamId),
                    Message = $"No team with ID '{request.TeamId}'.",
                });
                return;
            }

            // Check if is lecturer of class
            if (request.UserRole == RoleConstants.LECTURER)
            {
                if (team.LecturerId != request.UserId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{team.ClassId}'.",
                    });
                    return;
                }
            }
            // Check if is member of team
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                var isTeamMember = team.ClassMembers.Any(x => x.StudentId == request.UserId);
                if (!isTeamMember)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID '{team.TeamId}'.",
                    });
                    return;
                }
            }
        }
    }
}
