using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.TeamMilestones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestonesByTeam
{
    public class GetMilestonesByTeamHandler : QueryHandler<GetMilestonesByTeamQuery, GetMilestonesByTeamResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMilestonesByTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetMilestonesByTeamResult> HandleCommand(GetMilestonesByTeamQuery request, CancellationToken cancellationToken)
        {
            var result = new GetMilestonesByTeamResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                // Get milestones of team
                var milestones = await _unitOfWork.TeamMilestoneRepo.GetMilestonesByTeamId(request.TeamId);
                result.TeamMilestones = milestones.ToTeamMilestoneVM();

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetMilestonesByTeamQuery request)
        {
            // Check teamId
            var teamEntity = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
            if (teamEntity == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamId),
                    Message = $"No team with ID: {request.TeamId}",
                });
                return;
            }

            // Check if a Lecturer/Student in the class of this team
            if (request.UserRole == RoleConstants.LECTURER && teamEntity.Class.LecturerId != request.UserId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the lecturer of the class with ID '{request.TeamId}',",
                });
                return;
            }
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                // Get all studentIds in team
                var teamStudentIds = teamEntity.ClassMembers.Select(x => x.StudentId).ToHashSet();

                // Check if viewing student is in team
                if (!teamStudentIds.Any() || !teamStudentIds.Contains(request.UserId))
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID '{request.TeamId}',",
                    });
                    return;
                }
            }
        }
    }
}
