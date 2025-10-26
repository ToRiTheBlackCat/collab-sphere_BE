using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Commands.AssignLec
{
    public class AssignLecturerToClassHandler : CommandHandler<AssignLecturerToClassCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AssignLecturerToClassHandler> _logger;

        public AssignLecturerToClassHandler(IUnitOfWork unitOfWork,
                                 ILogger<AssignLecturerToClassHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(AssignLecturerToClassCommand request, CancellationToken cancellationToken)
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

                //Find class
                var foundClass = await _unitOfWork.ClassRepo.GetClassByIdAsync(request.ClassId);

                //Find lecturer
                var foundLecturer = await _unitOfWork.LecturerRepo.GetById(request.LecturerId);

                if (foundClass != null && foundLecturer != null)
                {
                    //Assign lecturer to class
                    foundClass.LecturerId = foundLecturer.LecturerId;
                    foundClass.LecturerName = foundLecturer.Fullname;
                    _unitOfWork.ClassRepo.Update(foundClass);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("Lecturer with ID: {LecturerId} assigned to class with ID: {ClassId} by user with ID: {UserId}",
                        request.LecturerId, request.ClassId, request.UserId);
                    result.IsSuccess = true;
                    result.Message = "Lecturer assigned to class successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning lecturer to class.");
                result.Message = "An error occurred while processing your request.";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, AssignLecturerToClassCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.STAFF };

            //Check if role is STAFF
            if (bypassRoles.Contains(request.UserRole))
            {
                //Check if class exists
                var foundClass = await _unitOfWork.ClassRepo.GetById(request.ClassId);
                if (foundClass == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "ClassId",
                        Message = $"Class with the given ID: {request.ClassId} does not exist."
                    });
                }

                //Check if lecturer exists
                var foundLecturer = await _unitOfWork.LecturerRepo.GetById(request.LecturerId);
                if (foundLecturer == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "LecturerId",
                        Message = $"Lecturer with the given ID: {request.LecturerId} does not exist."
                    });
                }
            }
            else
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"You do not have permission to assign lecturer to class."
                });
            }
        }
    }
}
