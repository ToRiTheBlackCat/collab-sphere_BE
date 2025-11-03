using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Team;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetAllTeamOfStudent
{
    public class GetAllTeamOfStudentHandler : QueryHandler<GetAllTeamOfStudentQuery, GetAllTeamOfStudentResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllTeamOfStudentHandler> _logger;
        private readonly CloudinaryService _cloudinaryService;

        public GetAllTeamOfStudentHandler(IUnitOfWork unitOfWork,
                                              ILogger<GetAllTeamOfStudentHandler> logger,
                                              CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }
        protected override async Task<GetAllTeamOfStudentResult> HandleCommand(GetAllTeamOfStudentQuery request, CancellationToken cancellationToken)
        {
            var result = new GetAllTeamOfStudentResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var teams = await _unitOfWork.TeamRepo.GetListTeamOfStudent(request.StudentId, request.TeamName, request.ClassId, request.SemesterId);
                if (teams == null || !teams.Any())
                {
                    result.IsSuccess = true;
                    result.Message = "No teams found for the specified student.";
                    result.PaginatedTeams = null;
                    return result;
                }

                var mappedTeams = teams.ListTeam_To_AllTeamOfStudentDto();
                foreach (var team in mappedTeams)
                {
                    team.TeamImage = await _cloudinaryService.GetImageUrl(team.TeamImage);
                }

                result.PaginatedTeams = new PagedList<AllTeamOfStudentDto>(
                   list: mappedTeams,
                   pageNum: request.PageNum,
                   pageSize: request.PageSize,
                   viewAll: request.ViewAll
               );
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting teams of student with ID: {StudentId}", request.StudentId);
                result.Message = "An error occurred while processing your request.";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllTeamOfStudentQuery request)
        {
            //Check view permision
            if (request.ViewerUId != request.StudentId)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ViewerUId),
                    Message = $"This user with ID: {request.ViewerUId} not has permission to get list of team of this student."
                });
            }

            //Check student exist
            var foundStudent = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.StudentId);
            if (foundStudent == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.StudentId),
                    Message = $"This student with ID: {request.StudentId} not exist!"
                });
            }

            //Check class exist
            if (request.ClassId != null)
            {
                var foundClass = await _unitOfWork.ClassRepo.GetById((int)request.ClassId);

                if (foundClass == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.ClassId),
                        Message = $"This class with ID: {request.ClassId} not exist!"
                    });
                }
            }

            //Check role permission
            if (request.ViewerRole != RoleConstants.STUDENT)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ViewerRole),
                    Message = $"This user with ID: {request.ViewerUId} not has permission to get list of team of this class."
                });
            }
        }
    }
}
