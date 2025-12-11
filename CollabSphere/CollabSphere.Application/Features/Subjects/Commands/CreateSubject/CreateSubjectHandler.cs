using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectHandler : CommandHandler<CreateSubjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateSubjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateSubjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var subjectDto = request.Subject;

                await _unitOfWork.BeginTransactionAsync();

                // Insert subject
                var subject = new Subject()
                {
                    SubjectName = subjectDto.SubjectName,
                    SubjectCode = subjectDto.SubjectCode,
                    IsActive = subjectDto.IsActive,
                };

                await _unitOfWork.SubjectRepo.Create(subject);
                await _unitOfWork.SaveChangesAsync();

                // Insert syllabus
                var syllabus = new SubjectSyllabus()
                {
                    SyllabusName = subjectDto.SubjectSyllabus.SyllabusName,
                    CreatedDate = DateTime.UtcNow,
                    Description = subjectDto.SubjectSyllabus.Description,
                    IsActive = subjectDto.SubjectSyllabus.IsActive,
                    NoCredit = subjectDto.SubjectSyllabus.NoCredit,
                    SubjectCode = subjectDto.SubjectCode,
                    //SubjectId = subject.SubjectId,
                    Subject = subject,
                };

                await _unitOfWork.SubjectSyllabusRepo.Create(syllabus);
                await _unitOfWork.SaveChangesAsync();

                // Insert grade components
                foreach (var gradeComponentDto in subjectDto.SubjectSyllabus.SubjectGradeComponents)
                {
                    await _unitOfWork.SubjectGradeComponentRepo.Create(new SubjectGradeComponent()
                    {
                        SyllabusId = syllabus.SyllabusId,

                        ComponentName = gradeComponentDto.ComponentName,
                        ReferencePercentage = gradeComponentDto.ReferencePercentage,
                        SubjectId = subject.SubjectId,
                    });
                }
                await _unitOfWork.SaveChangesAsync();

                // Insert Subject Outcome
                foreach (var subjectOutcomeDto in subjectDto.SubjectSyllabus.SubjectOutcomes)
                {
                    var outcome = new SubjectOutcome()
                    {
                        SyllabusId = syllabus.SyllabusId,

                        OutcomeDetail = subjectOutcomeDto.OutcomeDetail,
                    };
                    await _unitOfWork.SubjectOutcomeRepo.Create(outcome);
                    await _unitOfWork.SaveChangesAsync();

                    // Insert Syllabus Milestones for each Outcome
                    foreach (var syllabusMileDto in subjectOutcomeDto.SyllabusMilestones)
                    {
                        await _unitOfWork.SyllabusMilestoneRepo.Create(new SyllabusMilestone()
                        {
                            SyllabusId = syllabus.SyllabusId,
                            SubjectOutcomeId = outcome.SubjectOutcomeId,

                            Title = syllabusMileDto.Title,
                            Description = syllabusMileDto.Description,
                            StarDate = syllabusMileDto.StarDate,
                            EndDate = syllabusMileDto.EndDate,
                        });
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = $"Created subject '{subject.SubjectName}'({subject.SubjectId}) successfully.";
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
            var subjectDto = request.Subject;

            // Validate grade components
            var componentSum = subjectDto.SubjectSyllabus.SubjectGradeComponents.Sum(x => x.ReferencePercentage);
            if (componentSum != 100)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(subjectDto.SubjectSyllabus)}.{nameof(subjectDto.SubjectSyllabus.SubjectGradeComponents)}",
                    Message = $"Grade components do not sum up to 100."
                });
            }

            // Validate Subject Code
            var existSubject = await _unitOfWork.SubjectRepo.GetBySubjectCode(subjectDto.SubjectCode);
            if (existSubject != null)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(subjectDto.SubjectCode)}",
                    Message = $"A Subject with {nameof(subjectDto.SubjectCode)} '{subjectDto.SubjectCode}' already exist."
                });
            }
        }
    }
}
