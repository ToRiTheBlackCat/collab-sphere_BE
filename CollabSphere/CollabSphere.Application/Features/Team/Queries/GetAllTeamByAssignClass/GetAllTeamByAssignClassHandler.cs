using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Team;
using Microsoft.Extensions.Logging;

namespace CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass
{
    public class GetAllTeamByAssignClassHandler : QueryHandler<GetAllTeamByAssignClassQuery, GetAllTeamByAssignClassResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllTeamByAssignClassHandler> _logger;

        public GetAllTeamByAssignClassHandler(IUnitOfWork unitOfWork,
                                              ILogger<GetAllTeamByAssignClassHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        protected override async Task<GetAllTeamByAssignClassResult> HandleCommand(GetAllTeamByAssignClassQuery request, CancellationToken cancellationToken)
        {
            var result = new GetAllTeamByAssignClassResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var teams = await _unitOfWork.TeamRepo.SearchTeam(
                    classId: request.ClassId,
                    teamName: request.TeamName,
                    projectId: request.ProjectId,
                    fromDate: request.FromDate,
                    endDate: request.EndDate,
                    isDesc: request.IsDesc);

                var mappedTeams = teams.ListTeam_To_ListTeamByAssignClassDto();

                result.PaginatedTeams = new PagedList<AllTeamByAssignClassDto>(
                    list: mappedTeams,
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll
                );
                result.IsSuccess = true;

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                _logger.LogError(ex, "Error occurred while getting teams for class ID {ClassId}", request.ClassId);
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllTeamByAssignClassQuery request)
        {
            //Validate class
            var foundClass = await _unitOfWork.ClassRepo.GetById(request.ClassId);
            if (foundClass == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ClassId),
                    Message = $"Not found any class with that Id: {request.ClassId}"
                });
            }
            else
            {
                //Check role permission
                if (request.UserRole != RoleConstants.LECTURER || request.UserRole == RoleConstants.LECTURER && request.UserId != foundClass.LecturerId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserRole),
                        Message = $"This user with ID: {request.UserId} not has permission to get list of team of this class."
                    });
                }
            }
        }
    }
}
