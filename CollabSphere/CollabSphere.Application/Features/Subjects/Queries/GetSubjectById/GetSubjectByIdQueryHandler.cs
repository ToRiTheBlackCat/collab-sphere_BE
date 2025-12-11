using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectModels;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Subjects.Queries.GetSubjectById
{
    public class GetSubjectByIdQueryHandler : QueryHandler<GetSubjectByIdQuery, GetSubjectByIdResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSubjectByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetSubjectByIdResult> HandleCommand(GetSubjectByIdQuery request, CancellationToken cancellationToken)
        {
            var result = new GetSubjectByIdResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                var subject = (await _unitOfWork.SubjectRepo.GetSubjectDetail(request.SubjectId!.Value))!;
                if (subject.SubjectSyllabi.Any())
                {
                    var syllabus = subject.SubjectSyllabi.First();
                    syllabus.SyllabusMilestones = syllabus.SyllabusMilestones.OrderBy(x => x.StarDate).ToList();
                    syllabus.SubjectOutcomes = syllabus.SubjectOutcomes.OrderBy(x => x.SubjectOutcomeId).ToList();
                }

                result.Subject = (SubjectVM)subject;

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetSubjectByIdQuery request)
        {
            var subject = await _unitOfWork.SubjectRepo.GetById(request.SubjectId!.Value);
            if (subject == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.SubjectId),
                    Message = $"No subject with ID: {request.SubjectId}",
                });
            }
        }
    }
}
