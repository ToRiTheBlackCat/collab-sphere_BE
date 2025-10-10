using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetStudentClasses
{
    public class GetStudentClassesHandler : QueryHandler<GetStudentClassesQuery, GetStudentClassesResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentClassesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetStudentClassesResult> HandleCommand(GetStudentClassesQuery request, CancellationToken cancellationToken)
        {
            var result = new GetStudentClassesResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var classes = await _unitOfWork.ClassRepo.GetClassByStudentId(request.StudentId,
                    subjectIds: request.SubjectIds,
                    className: request.ClassName,
                    orderby: request.OrderBy,
                    descending: request.Descending);

                result.PaginatedClasses = new Common.PagedList<ClassVM>(
                    list: classes.Select(x => (ClassVM)x),
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll);

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetStudentClassesQuery request)
        {
            return;
        }
    }
}
