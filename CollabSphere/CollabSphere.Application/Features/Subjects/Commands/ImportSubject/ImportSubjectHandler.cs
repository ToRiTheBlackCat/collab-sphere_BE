using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    count++;

                    var testBefore = await _unitOfWork.SubjectRepo.GetAll();
                }
                await _unitOfWork.SaveChangesAsync();
                var testAfter = await _unitOfWork.SubjectRepo.GetAll();
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
            }
        }
    }
}
