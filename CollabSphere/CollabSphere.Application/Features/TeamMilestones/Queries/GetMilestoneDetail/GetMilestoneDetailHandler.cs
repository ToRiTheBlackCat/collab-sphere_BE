using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.DTOs.Validation;
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

        public GetMilestoneDetailHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                var milestone = await _unitOfWork.TeamMilestoneRepo.GetById(request.TeamMilestoneId);
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

                    result.TeamMilestone = (TeamMilestoneDetailDto)milestone;
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
