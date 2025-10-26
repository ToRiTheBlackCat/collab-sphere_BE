using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Classes.Queries.GetAllClasses;
using CollabSphere.Application.Mappings.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetLecturerClasses
{
    public class GetLecturerClassesHandler : QueryHandler<GetLecturerClassesQuery, GetLecturerClassesResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLecturerClassesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetLecturerClassesResult> HandleCommand(GetLecturerClassesQuery request, CancellationToken cancellationToken)
        {
            var result = new GetLecturerClassesResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var classes = await _unitOfWork.ClassRepo.GetClassByLecturerId(
                    lecturerId: request.LecturerId,
                    className: request.ClassName,
                    semesterId: request.SemesterId,
                    subjectIds: request.SubjectIds,
                    orderby: request.OrderBy,
                    descending: request.Descending);

                result.PaginatedClasses = new PagedList<ClassVM>(
                    list: classes.ToClassVM(true),
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

        protected override async Task ValidateRequest(List<OperationError> errors, GetLecturerClassesQuery request)
        {
            return;
        }
    }
}
