using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Commands.AddStudent
{
    public class AddStudentToClassHandler : CommandHandler<AddStudentToClassCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddStudentToClassHandler> _logger;

        public AddStudentToClassHandler(IUnitOfWork unitOfWork,
                                 ILogger<AddStudentToClassHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(AddStudentToClassCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            int addedCount = 0;
            StringBuilder rawMessage = new StringBuilder();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find existing class
                var existingClass = await _unitOfWork.ClassRepo.GetById(request.ClassId);

                //Find all classmembers in that class
                var classMembers = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassId(request.ClassId);

                foreach (var stu in request.StudentList)
                {
                    //Find existes student
                    var existingStudent = await _unitOfWork.StudentRepo.GetById(stu.StudentId);
                    if (existingStudent == null)
                    {
                        rawMessage.Append( $"Cannot add this student to class. Not found any student with Id: {stu.StudentId} | ");
                        continue;
                    }

                    //Check if student already in class
                    if (classMembers.Any() && classMembers.FirstOrDefault(x => x.StudentId == existingStudent.StudentId) != null)
                    {
                        rawMessage.Append($"Student {stu.StudentName} already in class {existingClass.ClassName}. Cannot add this student to class | ");
                        continue;
                    }

                    //Check if fullname is matching with existing student
                    if (!existingStudent.Fullname.Equals(stu.StudentName, StringComparison.OrdinalIgnoreCase))
                    {
                        rawMessage.Append($"Student name {stu.StudentName} does not match with existing student name {existingStudent.Fullname}. Cannot add this student to class | ");
                        continue;
                    }

                    //Create new class member if pass all validation
                    var newClassMember = new Domain.Entities.ClassMember
                    {
                        ClassId = existingClass.ClassId,
                        StudentId = existingStudent.StudentId,
                        Fullname = existingStudent.Fullname,
                        IsGrouped = false,
                        Status = 1 //Active
                    };
                    await _unitOfWork.ClassMemberRepo.Create(newClassMember);
                    await _unitOfWork.SaveChangesAsync();

                    rawMessage.Append($"Added student {existingStudent.Fullname} to class {existingClass.ClassName} successfully. | ");
                    addedCount++;
                }
                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                rawMessage.Append($"Added total {addedCount} students to class with ID: {request.ClassId} | ");
                result.Message = rawMessage.ToString();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while adding student to class.");
                result.IsSuccess = false;
                result.Message = "An error occurred while adding student to class";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, AddStudentToClassCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.STAFF };

            //Validate class
            var foundClass = await _unitOfWork.ClassRepo.GetById(request.ClassId);
            if (foundClass == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ClassId),
                    Message = $"Not found any class with that Id: {request.ClassId}"
                });
                return;
            }

            //Check if role is valid to add student to class
            if (bypassRoles.Contains(request.UserRole))
            {
                if (request.StudentList == null || !request.StudentList.Any())
                {
                    errors.Add(new OperationError()
                    {
                        Field = "StudentList",
                        Message = $"Student list cannot be empty."
                    });
                    return;
                }
            }
            else
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"You do not have permission to assign lecturer to class."
                });
                return;
            }
        }
    }
}
