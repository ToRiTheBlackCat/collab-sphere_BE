using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetAllClasses
{
    public class GetAllClassesHandler : QueryHandler<GetAllClassesQuery, GetAllClassesResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllClassesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetAllClassesResult> HandleCommand(GetAllClassesQuery request, CancellationToken cancellationToken)
        {
            var result = new GetAllClassesResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var classes = await _unitOfWork.ClassRepo.SearchClasses(
                    className: request.ClassName,
                    lecturerIds: request.LecturerIds,
                    subjectIds: request.SubjectIds,
                    orderby: request.OrderBy,
                    descending: request.Descending);

                result.PaginatedClasses = new PagedList<ClassVM>(
                    list: classes.Select(x => (ClassVM)x),
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll
                    );
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllClassesQuery request)
        {
            return;
        }
    }
}
