using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Semesters.Commands.DeleteSemester
{
    public class DeleteSemesterHandler : CommandHandler<DeleteSemesterCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private Semester _foundSemester;
        public DeleteSemesterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteSemesterCommand request, CancellationToken cancellationToken)
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

                    _unitOfWork.SemesterRepo.Delete(_foundSemester);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = "Semester deleted successfully.";
                
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = $"Fail to delete this semester. Error Detail: {ex.Message}";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteSemesterCommand request)
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
                        Message = $"Cannot delete this Semester '{foundSemester.SemesterName}' because there are classes associated with this semester."
                    });
                }
                _foundSemester = foundSemester;
                return;
            }
        }
    }
}
