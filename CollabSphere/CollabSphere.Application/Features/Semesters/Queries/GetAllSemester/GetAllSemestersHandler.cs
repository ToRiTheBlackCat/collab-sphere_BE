using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Semesters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Semesters.Queries.GetAllSemester
{
    public class GetAllSemestersHandler : QueryHandler<GetAllSemestersQuery, GetAllSemestersResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllSemestersHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetAllSemestersResult> HandleCommand(GetAllSemestersQuery request, CancellationToken cancellationToken)
        {
            var result = new GetAllSemestersResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message  = string.Empty,
            };

            try
            {
                var semesters = await _unitOfWork.SemesterRepo.GetAll();

                result.Semesters = semesters.ToSemesterVM();
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllSemestersQuery request)
        {
            return;
        }
    }
}
