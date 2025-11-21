using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.Application.Features.Team.Queries.GetTeamProjectOverview
{
    public class GetTeamProjectOverviewHandler : QueryHandler<GetTeamProjectOverviewQuery, GetTeamProjectOverviewResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTeamProjectOverviewHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetTeamProjectOverviewResult> HandleCommand(GetTeamProjectOverviewQuery request, CancellationToken cancellationToken)
        {
            var result = new GetTeamProjectOverviewResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                // Get Team's detail
                var team = await _unitOfWork.TeamRepo.GetTeamWithProjectOverview(request.TeamId);

                result.ProjectOverview = team!.ExtractProjectInfo();
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetTeamProjectOverviewQuery request)
        {
            // Get team
            var team = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
            if (team == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamId),
                    Message = $"No team with ID '{request.TeamId}' found.",
                });
                return;
            }

            // Check if is assigned lecturer
            if (request.UserRole == RoleConstants.LECTURER && team.Class.LecturerId != request.UserId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{request.TeamId}'.",
                });
                return;
            }
            // Check if is member in team
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                // Get all studentIds in team
                var teamStudentIds = team.ClassMembers.Select(x => x.StudentId).ToHashSet();

                // Check if viewing student is in team
                if (!teamStudentIds.Any() || !teamStudentIds.Contains(request.UserId))
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID '{request.TeamId}'.",
                    });
                    return;
                }
            }

            // Check if team is assigned a project
            if (!team.ProjectAssignmentId.HasValue)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamId),
                    Message = $"This team has't been assigned a project.",
                });
                return;
            }
        }
    }
}
