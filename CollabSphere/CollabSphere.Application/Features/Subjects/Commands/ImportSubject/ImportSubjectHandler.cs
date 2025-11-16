using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Subjects.Commands.ImportSubject
{
    public class ImportSubjectHandler : CommandHandler<ImportSubjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public ImportSubjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(ImportSubjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data operations
                // Add Subjects
                int count = 0;
                foreach (var subjectDto in request.Subjects)
                {
                    var subject = new Subject()
                    {
                        SubjectCode = subjectDto.SubjectCode,
                        SubjectName = subjectDto.SubjectName,
                        IsActive = subjectDto.IsActive,
                    };
                    await _unitOfWork.SubjectRepo.Create(subject);
                    await _unitOfWork.SaveChangesAsync();

                    // Add subject syllabus
                    if (subjectDto.SubjectSyllabus != null)
                    {
                        var syllabusDto = subjectDto.SubjectSyllabus;
                        var syllabus = new SubjectSyllabus()
                        {
                            SyllabusName = syllabusDto.SyllabusName,
                            Description = syllabusDto.Description,
                            NoCredit = syllabusDto.NoCredit,
                            IsActive = syllabusDto.IsActive,
                            CreatedDate = DateTime.UtcNow,
                            Subject = subject,
                            SubjectCode = subjectDto.SubjectCode,
                        };
                        await _unitOfWork.SubjectSyllabusRepo.Create(syllabus);
                        await _unitOfWork.SaveChangesAsync();

                        // Add subject outcomes
                        foreach (var outcomeDto in syllabusDto.SubjectOutcomes)
                        {
                            var outcome = new SubjectOutcome()
                            {
                                OutcomeDetail = outcomeDto.OutcomeDetail,
                                Syllabus = syllabus,
                            };
                            await _unitOfWork.SubjectOutcomeRepo.Create(outcome);
                        }
                        await _unitOfWork.SaveChangesAsync();

                        // Add subject grade components
                        foreach (var componentDto in syllabusDto.SubjectGradeComponents)
                        {
                            var component = new SubjectGradeComponent()
                            {
                                ComponentName = componentDto.ComponentName,
                                ReferencePercentage = componentDto.ReferencePercentage,
                                SubjectId = subject.SubjectId,
                                Syllabus = syllabus,
                            };
                            await _unitOfWork.SubjectGradeComponentRepo.Create(component);
                        }
                        await _unitOfWork.SaveChangesAsync();
                    }


                    count++;
                }
                #endregion

                await _unitOfWork.CommitTransactionAsync();
                result.IsSuccess = true;
                result.Message = $"Successfully imported {count} subject(s).";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, ImportSubjectCommand request)
        {
            // Check for empty subject list
            if (!request.Subjects.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.Subjects),
                    Message = "There are no subjects to be imported"
                });

                return;
            }

            // Check for SubjectCode duplicates in request's subject list
            var duplicatedCodes = request.Subjects
                .GroupBy(x => x.SubjectCode)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();
            if (duplicatedCodes.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.Subjects),
                    Message = $"Duplicated SubjectCodes found in request: {string.Join(", ", duplicatedCodes)}"
                });

                return;
            }

            var subjects = await _unitOfWork.SubjectRepo.GetAll();

            // Check individual subject DTO
            var subjectDtosCount = request.Subjects.Count();
            for (int index = 0; index < subjectDtosCount; index++)
            {
                var subjectDto = request.Subjects[index];

                // Check for SubjectCode duplicate in DB
                var existSubject = subjects
                    .FirstOrDefault(x => x.SubjectCode.Equals(subjectDto.SubjectCode));
                if (existSubject != null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"{nameof(request.Subjects)}[{index}]",
                        Message = $"Subject with SubjectCode '{subjectDto.SubjectCode}' already exist in DB."
                    });
                }

                if (subjectDto.SubjectSyllabus == null) // Skips over when subject doesn't include syllabus
                {
                    continue;
                }

                // Check empty Grade components
                if (!subjectDto.SubjectSyllabus.SubjectGradeComponents.Any()) 
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"{nameof(request.Subjects)}[{index}].{nameof(subjectDto.SubjectSyllabus.SubjectGradeComponents)}",
                        Message = $"Can't be an empty sequence."
                    });
                }
                else
                {
                    // Check for grade components sum
                    var percentSum = subjectDto.SubjectSyllabus.SubjectGradeComponents.Sum(x => x.ReferencePercentage);
                    if (percentSum != 100)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = $"{nameof(request.Subjects)}[{index}].{nameof(subjectDto.SubjectSyllabus.SubjectGradeComponents)}",
                            Message = $"{nameof(subjectDto.SubjectSyllabus.SubjectGradeComponents)} don't sum up to 100%."
                        });
                    }
                }

                // Check empty outcomes
                if (!subjectDto.SubjectSyllabus.SubjectOutcomes.Any())
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"{nameof(request.Subjects)}[{index}].{nameof(subjectDto.SubjectSyllabus.SubjectOutcomes)}",
                        Message = $"Can't be an empty sequence."
                    });
                }
            }
        }
    }
}
