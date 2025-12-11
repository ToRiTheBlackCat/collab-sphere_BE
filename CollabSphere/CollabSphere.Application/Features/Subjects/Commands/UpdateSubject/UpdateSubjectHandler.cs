using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectHandler : CommandHandler<UpdateSubjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateSubjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateSubjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var subject = await _unitOfWork.SubjectRepo.GetSubjectDetail(request.SubjectId);
                _unitOfWork.SubjectRepo.Update(subject!);

                // Update Syllabus
                var syllabusDto = request.Subject.SubjectSyllabus;

                var syllabus = subject!.SubjectSyllabi.First();

                var states = await _unitOfWork.GetStates();

                // Replace grade components
                syllabus.SubjectGradeComponents.Clear();

                foreach (var gradeComponentDto in syllabusDto.SubjectGradeComponents)
                {
                    syllabus.SubjectGradeComponents.Add(new SubjectGradeComponent()
                    {
                        ComponentName = gradeComponentDto.ComponentName,
                        ReferencePercentage = gradeComponentDto.ReferencePercentage,
                        SubjectId = subject.SubjectId,
                        SyllabusId = syllabus.SyllabusId,
                    });
                }

                // Replace subject outcomes
                syllabus.SubjectOutcomes.Clear();

                // Create Subject Outcomes
                foreach (var outcomeDto in syllabusDto.SubjectOutcomes)
                {
                    var outcome = new SubjectOutcome()
                    {
                        OutcomeDetail = outcomeDto.OutcomeDetail,
                        SyllabusMilestones = outcomeDto.SyllabusMilestones.Select(m => new SyllabusMilestone
                        {
                            SyllabusId = syllabus.SyllabusId,

                            Title = m.Title,
                            Description = m.Description,
                            StarDate = m.StarDate,
                            EndDate = m.EndDate
                        }).ToList()
                    };

                    syllabus.SubjectOutcomes.Add(outcome);
                }

                // Update syllabus
                syllabus!.SyllabusName = syllabusDto.SyllabusName;
                syllabus.IsActive = syllabusDto.IsActive;
                syllabus.Description = syllabusDto.Description;
                syllabus.NoCredit = syllabusDto.NoCredit;
                syllabus.SubjectCode = subject.SubjectCode;

                // Update subject
                subject!.SubjectId = request.SubjectId;
                subject.SubjectCode = request.Subject.SubjectCode;
                subject.SubjectName = request.Subject.SubjectName;
                subject.IsActive = request.Subject.IsActive;

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = "Updated subject successfully.";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateSubjectCommand request)
        {
            // Check exist subject
            var subject = await _unitOfWork.SubjectRepo.GetById(request.SubjectId);
            if (subject == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.SubjectId),
                    Message = $"Subject with ID = '{request.SubjectId}' doesn't exist."
                });
            }

            var syllabusDto = request.Subject.SubjectSyllabus;

            // Validate grade components
            var componentSum = syllabusDto.SubjectGradeComponents.Sum(x => x.ReferencePercentage);
            if (componentSum != 100)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.Subject.SubjectSyllabus)}.{nameof(request.Subject.SubjectSyllabus.SubjectGradeComponents)}",
                    Message = $"{nameof(request.Subject.SubjectSyllabus.SubjectGradeComponents)} don't sum up to 100."
                });
            }

            // Validate Subject Code
            var existingSubject = await _unitOfWork.SubjectRepo.GetBySubjectCode(request.Subject.SubjectCode);
            if (existingSubject != null && existingSubject.SubjectId != request.SubjectId)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.Subject.SubjectCode)}",
                    Message = $"Subject with {nameof(request.Subject.SubjectCode)} '{request.Subject.SubjectCode}' already exist."
                });
            }
        }
    }
}
