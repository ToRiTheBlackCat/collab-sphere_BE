using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                // Update Subject
                var subject = await _unitOfWork.SubjectRepo.GetById(request.SubjectId);
                subject!.SubjectId = request.SubjectId;
                subject.SubjectCode = request.SubjectCode;
                subject.SubjectName = request.SubjectName;
                subject.IsActive = request.IsActive;

                _unitOfWork.SubjectRepo.Update(subject);
                await _unitOfWork.SaveChangesAsync();

                // Update Syllabus
                var syllabus = subject.SubjectSyllabi.First();
                syllabus!.SyllabusName = request.SubjectSyllabus.SyllabusName;
                syllabus.IsActive = request.SubjectSyllabus.IsActive;
                syllabus.Description = request.SubjectSyllabus.Description;
                syllabus.NoCredit = request.SubjectSyllabus.NoCredit;
                syllabus.SubjectCode = subject.SubjectCode;

                _unitOfWork.SubjectSyllabusRepo.Update(syllabus);
                await _unitOfWork.SaveChangesAsync();

                // Replace grade components
                var existingComponents = await _unitOfWork.SubjectGradeComponentRepo.GetAll();
                var componentsToDelete = existingComponents.Where(x => x.SyllabusId == syllabus.SyllabusId).ToList();
                foreach (var comp in componentsToDelete)
                {
                    _unitOfWork.SubjectGradeComponentRepo.Delete(comp);
                    await _unitOfWork.SaveChangesAsync();
                }

                foreach (var gradeComponentDto in request.SubjectSyllabus.SubjectGradeComponents)
                {
                    await _unitOfWork.SubjectGradeComponentRepo.Create(new SubjectGradeComponent()
                    {
                        ComponentName = gradeComponentDto.ComponentName,
                        ReferencePercentage = gradeComponentDto.ReferencePercentage,
                        SubjectId = subject.SubjectId,
                        Syllabus = syllabus,
                    });
                    await _unitOfWork.SaveChangesAsync();
                }

                // Replace subject outcomes
                var existingOutcomes = await _unitOfWork.SubjectOutcomeRepo.GetAll();
                var outcomesToDelete = existingOutcomes.Where(x => x.SyllabusId == syllabus.SyllabusId).ToList();
                foreach (var outcome in outcomesToDelete)
                {
                    _unitOfWork.SubjectOutcomeRepo.Delete(outcome);
                    await _unitOfWork.SaveChangesAsync();
                }

                foreach (var outcomeDto in request.SubjectSyllabus.SubjectOutcomes)
                {
                    await _unitOfWork.SubjectOutcomeRepo.Create(new SubjectOutcome()
                    {
                        OutcomeDetail = outcomeDto.OutcomeDetail,
                        Syllabus = syllabus
                    });
                    await _unitOfWork.SaveChangesAsync();
                }

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

            // Validate grade components
            var componentSum = request.SubjectSyllabus.SubjectGradeComponents.Sum(x => x.ReferencePercentage);
            if (componentSum != 100)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.SubjectSyllabus)}.{nameof(request.SubjectSyllabus.SubjectGradeComponents)}",
                    Message = $"{nameof(request.SubjectSyllabus.SubjectGradeComponents)} don't sum up to 100."
                });
            }

            // Validate Subject Code
            var existSubjectCode = await _unitOfWork.SubjectRepo.GetBySubjectCode(request.SubjectCode);
            if (existSubjectCode != null && existSubjectCode.SubjectId != request.SubjectId)
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
