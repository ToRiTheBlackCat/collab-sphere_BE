using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectModels;
using CollabSphere.Application.DTOs.Validation;

namespace CollabSphere.Application.Features.Subjects.GetAllSubject
{
    public class GetAllSubjectsQueryHandler : QueryHandler<GetAllSubjectsQuery, GetAllSubjectResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllSubjectsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetAllSubjectResult> HandleCommand(GetAllSubjectsQuery request, CancellationToken cancellationToken)
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

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllSubjectsQuery request)
        {
        }
    }
}
