using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Commands.CreateClass
{
    public class CreateClassCommandHandler : CommandHandler<CreateClassCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateClassCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateClassCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            // Start operation
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Insert Class
                var lecturer = await _unitOfWork.LecturerRepo.GetById(request.LecturerId);
                var addClass = new Class()
                {
                    ClassName = request.ClassName,
                    EnrolKey = request.EnrolKey,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = request.IsActive,
                    LecturerId = request.LecturerId,
                    LecturerName = lecturer!.Fullname,
                    SubjectId = request.SubjectId,
                    MemberCount = request.StudentIds.Count(),
                    TeamCount = 0,
                };

                await _unitOfWork.ClassRepo.Create(addClass);
                await _unitOfWork.SaveChangesAsync();

                // Insert Class Members
                foreach (var studentId in request.StudentIds)
                {
                    var student = await _unitOfWork.StudentRepo.GetById(studentId);
                    var classMember = new ClassMember()
                    {
                        Class = addClass,
                        Fullname = student!.Fullname,
                        StudentId = studentId,
                        IsGrouped = false,
                        Status = 1,
                    };
                    
                    await _unitOfWork.ClassMemberRepo.Create(classMember);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = "Class created successfully";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateClassCommand request)
        {
            #region Validate request
            // Check subject
            var subject = await _unitOfWork.SubjectRepo.GetById(request.SubjectId);
            if (subject == null)
            {
                var error = new OperationError()
                {
                    Field = nameof(request.SubjectId),
                    Message = $"No subject with ID: {request.SubjectId}"
                };
                errors.Add(error);
            }

            // Check lecturer
            var lecturer = await _unitOfWork.LecturerRepo.GetById(request.LecturerId);
            if (lecturer == null)
            {
                var error = new OperationError()
                {
                    Field = nameof(request.LecturerId),
                    Message = $"No lecturer with ID: {request.LecturerId}"
                };
                errors.Add(error);
            }

            // Check students
            //var allStudents = await _unitOfWork.StudentRepo.GetAll();
            for (int index = 0; index < request.StudentIds.Count; index++)
            {
                var studentId = request.StudentIds[index];
                var student = await _unitOfWork.StudentRepo.GetById(studentId);

                if (student == null)
                {
                    var error = new OperationError()
                    {
                        Field = $"{nameof(request.StudentIds)}[{index}]",
                        Message = $"No Student with ID '{studentId}' exist."
                    };
                    errors.Add(error);
                }
            }
            #endregion
        }
    }
}
