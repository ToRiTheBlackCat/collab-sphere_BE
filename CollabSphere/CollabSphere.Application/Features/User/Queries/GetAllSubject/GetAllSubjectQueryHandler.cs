using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Model;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Queries.GetAllSubject
{
    public class GetAllSubjectQueryHandler : BaseQueryHandler<GetAllSubjectQuery, GetAllSubjectResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllSubjectQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetAllSubjectResult> HandleCommand(GetAllSubjectQuery request, CancellationToken cancellationToken)
        {
            var result = new GetAllSubjectResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = "",
                ErrorList = new(),
                Subjects = new()
            };

            try
            {
                var subjects = await _unitOfWork.SubjectRepo.GetAll();

                result.Subjects = subjects.Select(x => (SubjectVM)x).ToList();
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllSubjectQuery request)
        {
        }
    }
}
