using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Semesters.Commands.UpdateSemester
{
    public class UpdateSemesterHandler : CommandHandler<UpdateSemesterCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateSemesterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(UpdateSemesterCommand request, CancellationToken cancellationToken)
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

                var foundSemester = await _unitOfWork.SemesterRepo.GetById(request.SemesterId);
                if (foundSemester != null)
                {
                    foundSemester.SemesterName = request.SemesterName.Trim();
                    foundSemester.SemesterCode = request.SemesterCode.Trim();
                    foundSemester.StartDate = request.StartDate;
                    foundSemester.EndDate = request.EndDate;

                    _unitOfWork.SemesterRepo.Update(foundSemester);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = "Semester updated successfully.";
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = $"Fail to update this semester. Error Detail: {ex.Message}";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateSemesterCommand request)
        {
            var foundSemester = await _unitOfWork.SemesterRepo.GetById(request.SemesterId);
            if (foundSemester == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.SemesterId),
                    Message = $"Not found any semester with that Id: {request.SemesterId}"
                });
                return;
            }
            else
            {
                var foundClasses = await _unitOfWork.SemesterRepo.GetClassesBySemester(request.SemesterId);
                if (foundClasses != null && foundClasses.Any())
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.SemesterId),
                        Message = $"Cannot update this Semester '{foundSemester.SemesterName}' because there are classes associated with this semester."
                    });
                }
                return;
            }
        }
    }
}
