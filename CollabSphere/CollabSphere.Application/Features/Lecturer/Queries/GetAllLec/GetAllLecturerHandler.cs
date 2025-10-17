using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using CollabSphere.Application.Mappings.Lecturer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Queries.GetAllLec
{
    public class GetAllLecturerHandler : QueryHandler<GetAllLecturerQuery, GetAllLecturerResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllLecturerHandler> _logger;

        public GetAllLecturerHandler(IUnitOfWork unitOfWork,
                                              ILogger<GetAllLecturerHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<GetAllLecturerResult> HandleCommand(GetAllLecturerQuery request, CancellationToken cancellationToken)
        {
            var result = new GetAllLecturerResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var lecturerList = await _unitOfWork.LecturerRepo
                  .SearchLecturer(
                       request.Email,
                       request.FullName,
                       request.Yob,
                       request.LecturerCode,
                       request.Major,
                       request.IsDesc
                  );

                if (lecturerList == null || !lecturerList.Any())
                {
                    result.IsSuccess = true;
                    result.Message = "No lecturer found.";
                    result.PaginatedLecturers = null;
                    return result;
                }

                var mappedList = lecturerList.ListUser_To_LecturerResponseDto();
                result.PaginatedLecturers = new PagedList<LecturerResponseDto>(
                   list: mappedList,
                   pageNum: request.PageNum,
                   pageSize: request.PageSize,
                   viewAll: request.ViewAll
               );
                result.IsSuccess = true;
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while getting list of Lecturer");
                result.Message = "An error occurred while processing your request.";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllLecturerQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.STAFF };

            //Check role permission
            if (!bypassRoles.Contains(request.UserRole))
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"This role with ID: {request.UserRole} not has permission to get this Lecturer list."
                });
                return;
            }
        }
    }
}
