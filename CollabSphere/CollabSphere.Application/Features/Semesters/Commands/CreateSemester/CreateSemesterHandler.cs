using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Semesters.Commands.CreateSemester
{
    public class CreateSemesterHandler : CommandHandler<CreateSemesterCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateSemesterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateSemesterCommand request, CancellationToken cancellationToken)
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

                var existedSemester = await _unitOfWork.SemesterRepo.GetSemesterWithNameAndCode(request.SemesterName, request.SemesterCode);
                if (existedSemester != null)
                {
                    result.Message = $"Semester with name '{request.SemesterName}' or code '{request.SemesterCode}' already exists.";
                    return result;
                }

                var newSemester = new Domain.Entities.Semester()
                {
                    SemesterName = request.SemesterName.Trim(),
                    SemesterCode = request.SemesterCode.Trim(),
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };
                await _unitOfWork.SemesterRepo.Create(newSemester);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = "New semester created successfully.";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = $"Fail to create new semester. Error Detail: {ex.Message}";
            }

            return result;
        }

        protected override Task ValidateRequest(List<OperationError> errors, CreateSemesterCommand request)
        {
            return Task.CompletedTask;
        }
    }
}
