using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Google.Apis.Drive.v3.Data;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

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
                    EnrolKey =  GenerateRandomEnrolKey(6),
                    CreatedDate = DateTime.UtcNow,
                    SubjectId = request.SubjectId,
                    SemesterId = request.SemesterId,
                    IsActive = request.IsActive,
                    LecturerId = request.LecturerId,
                    LecturerName = lecturer!.Fullname,
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
                }
                await _unitOfWork.SaveChangesAsync();

                // Generate chat conversation for class (lecturer & students)
                var lecUser = await _unitOfWork.UserRepo.GetOneByUserIdAsync(request.LecturerId);
                var studentUsers = new List<Domain.Entities.User>();
                foreach (var studentId in request.StudentIds)
                {
                    var stuUser = await _unitOfWork.UserRepo.GetOneByUserIdAsync(studentId);
                    studentUsers.Add(stuUser!);
                }

                var chatUsers = new List<CollabSphere.Domain.Entities.User>() { lecUser! };
                chatUsers.AddRange(studentUsers);
                var chatConv = new ChatConversation()
                {
                    ClassId = addClass.ClassId,
                    ConversationName = addClass.ClassName,
                    TeamId = null,
                    CreatedAt = DateTime.UtcNow,
                    Users = chatUsers,
                };
                await _unitOfWork.ChatConversationRepo.Create(chatConv);
                await _unitOfWork.SaveChangesAsync();

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

        private string GenerateRandomEnrolKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using var rng = RandomNumberGenerator.Create();
            var result = new char[length];
            var buffer = new byte[sizeof(uint)];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                result[i] = chars[(int)(num % (uint)chars.Length)];
            }

            return new string(result);
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
                    Message = $"No subject with ID '{request.SubjectId}' exist."
                };
                errors.Add(error);
            }

            // Check semester
            var semester = await _unitOfWork.SemesterRepo.GetById(request.SemesterId);
            if (semester == null)
            {
                var error = new OperationError()
                {
                    Field = nameof(request.SemesterId),
                    Message = $"No semester with ID '{request.SemesterId}' exist."
                };
                errors.Add(error);
            }

            if (semester != null && subject != null)
            {
                // Can not create duplicated classes (Same Name, Semester, Subject)
                var duplicatedClass = await _unitOfWork.ClassRepo.GetDuplicatedClass(
                    request.ClassName,
                    request.SubjectId,
                    request.SemesterId
                );
                if (duplicatedClass != null)
                {
                    var error = new OperationError()
                    {
                        Field = nameof(request.LecturerId),
                        Message = $"There is already a class '{request.ClassName}' of subject '{subject.SubjectName}'({subject.SubjectId}) in semester '{semester.SemesterName}'({semester.SemesterId})."
                    };
                    errors.Add(error);
                }
            }

            // Check lecturer
            var lecturer = await _unitOfWork.LecturerRepo.GetById(request.LecturerId);
            if (lecturer == null)
            {
                var error = new OperationError()
                {
                    Field = nameof(request.LecturerId),
                    Message = $"No lecturer with ID '{request.LecturerId}' exist."
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
