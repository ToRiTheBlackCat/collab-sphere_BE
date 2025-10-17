using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.Student;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Lecturer.Queries.GetAllLec;
using CollabSphere.Application.Mappings.Student;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Queries.GetAllStudent
{
    public class GetAllStudentHandler : QueryHandler<GetAllStudentQuery, GetAllStudentResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllStudentHandler> _logger;

        public GetAllStudentHandler(IUnitOfWork unitOfWork,
                                              ILogger<GetAllStudentHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<GetAllStudentResult> HandleCommand(GetAllStudentQuery request, CancellationToken cancellationToken)
        {
            var result = new GetAllStudentResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var studentList = await _unitOfWork.StudentRepo
                    .SearchStudent(
                         request.Email,
                         request.FullName,
                         request.Yob,
                         request.StudentCode,
                         request.Major,
                         request.IsDesc
                    );
                if (studentList == null || !studentList.Any())
                {
                    result.IsSuccess = true;
                    result.Message = "No lecturer found.";
                    result.PaginatedStudents = null;
                    return result;
                }

                var mappedList = studentList.ListUser_To_StudentResponseDto();
                result.PaginatedStudents = new PagedList<StudentResponseDto>(
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
                _logger.LogError(ex, "Error occurred while getting list of Lecturer");
                result.Message = "An error occurred while processing your request.";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllStudentQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.STAFF };

            //Check role permission
            if (!bypassRoles.Contains(request.UserRole))
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"This role with ID: {request.UserRole} not has permission to get this Student list."
                });
                return;
            }
        }
    }
}
