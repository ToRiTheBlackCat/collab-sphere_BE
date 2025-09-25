using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Academic.Commands.CreateSubject
{
    public class CreateSubjectHandler : BaseCommandHandler<CreateSubjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateSubjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<BaseCommandResult> HandleCommand(CreateSubjectCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseCommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Insert subject
                var subject = new Subject()
                {
                    SubjectName = request.SubjectName,
                    SubjectCode = request.SubjectCode,
                    IsActive = request.IsActive,
                };

                await _unitOfWork.SubjectRepo.Create(subject);
                await _unitOfWork.SaveChangesAsync();

                // Insert syllabus
                var syllabus = new SubjectSyllabus()
                {
                    SyllabusName = request.SubjectSyllabus.SyllabusName,
                    CreatedDate = DateTime.UtcNow,
                    Description = request.SubjectSyllabus.Description,
                    IsActive = request.SubjectSyllabus.IsActive,
                    NoCredit = request.SubjectSyllabus.NoCredit,
                    SubjectCode = request.SubjectCode,
                    //SubjectId = subject.SubjectId,
                    Subject = subject,
                };

                await _unitOfWork.SubjectSyllabusRepo.Create(syllabus);
                await _unitOfWork.SaveChangesAsync();

                // Insert grade components
                foreach (var gradeComponentDto in request.SubjectSyllabus.SubjectGradeComponents)
                {
                    await _unitOfWork.SubjectGradeComponentRepo.Create(new SubjectGradeComponent()
                    {
                        ComponentName = gradeComponentDto.ComponentName,
                        ReferencePercentage = gradeComponentDto.ReferencePercentage,
                        SubjectId = subject.SubjectId,
                        //SyllabusId = syllabus.SyllabusId,
                        Syllabus = syllabus
                    });
                    await _unitOfWork.SaveChangesAsync();
                }

                // Insert Subject Outcome
                foreach (var subjectOutcomeDto in request.SubjectSyllabus.SubjectOutcomes)
                {
                    await _unitOfWork.SubjectOutcomeRepo.Create(new SubjectOutcome()
                    {
                        OutcomeDetail = subjectOutcomeDto.OutcomeDetail,
                        //SyllabusId = syllabus.SyllabusId,
                        Syllabus = syllabus,
                    });
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = "Subject created successfully.";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateSubjectCommand request)
        {
            // Validate grade components
            var componentTotal = request.SubjectSyllabus.SubjectGradeComponents.Sum(x => x.ReferencePercentage);
            if (componentTotal != 100)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.SubjectSyllabus)}.{nameof(request.SubjectSyllabus.SubjectGradeComponents)}",
                    Message = $"{nameof(request.SubjectSyllabus.SubjectGradeComponents)} don't sum up to 100."
                });
            }

            // Validate Subject Code
            var existSubject = (await _unitOfWork.SubjectRepo.GetAll())
                .FirstOrDefault(x => x.SubjectCode.ToUpper().Equals(request.SubjectCode.Trim().ToUpper()));
            if (existSubject != null)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.SubjectCode)}",
                    Message = $"Subject with {nameof(request.SubjectCode)} '{request.SubjectCode}' already exist."
                });
            }
        }
    }
}
