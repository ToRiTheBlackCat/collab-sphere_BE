using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Classes.Commands.ImportClass
{
    public class ImportClassHandler : CommandHandler<ImportClassCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public ImportClassHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(ImportClassCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            var lecturerList = await _unitOfWork.LecturerRepo.GetAll();
            var subjectList = await _unitOfWork.SubjectRepo.GetAll();
            var studentList = await _unitOfWork.StudentRepo.GetAll();
            var semesterList = await _unitOfWork.SemesterRepo.GetAll();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operations
                foreach (var classDto in request.Classes)
                {
                    // Find subject
                    var subject = subjectList.First(x => x.SubjectCode == classDto.SubjectCode);

                    // Find semester
                    var semester = semesterList.First(x => x.SemesterCode == classDto.SemesterCode);

                    // Find lecturer
                    var lecturer = lecturerList.First(x => x.LecturerCode == classDto.LecturerCode);

                    // Find students
                    var students = studentList.Where(x => classDto.StudentCodes.Contains(x.StudentCode));
                    var studentCount = students.Count();

                    if (studentCount != classDto.StudentCodes.Count)
                    {
                        throw new Exception("Data Operation Exception.");
                    }

                    // Insert class
                    var newClass = new Class()
                    {
                        ClassName = classDto.ClassName,
                        CreatedDate = DateTime.UtcNow,
                        EnrolKey =  GenerateRandomEnrolKey(6),
                        SubjectId = subject.SubjectId,
                        SemesterId = semester.SemesterId,
                        LecturerId = lecturer.LecturerId,
                        LecturerName = lecturer.Fullname,
                        MemberCount = studentCount,
                        TeamCount = 0,
                        IsActive = classDto.IsActive,
                    };
                    await _unitOfWork.ClassRepo.Create(newClass);
                    await _unitOfWork.SaveChangesAsync();

                    // Insert class members
                    foreach (var student in students)
                    {
                        var classMember = new ClassMember()
                        {
                            ClassId = newClass.ClassId,
                            Class = newClass,
                            StudentId = student.StudentId,
                            Fullname = student.Fullname,
                            Student = student,
                        };
                        await _unitOfWork.ClassMemberRepo.Create(classMember);
                    }
                    await _unitOfWork.SaveChangesAsync();

                    // Also create class conversation for lecturer & class students
                    var lecturerUser = await _unitOfWork.UserRepo.GetOneByUserIdAsync(lecturer.LecturerId);
                    var chatUsers = new List<CollabSphere.Domain.Entities.User>() { lecturerUser! };
                    foreach (var student in students)
                    {
                        var studentUser = await _unitOfWork.UserRepo.GetOneByUserIdAsync(student.StudentId);
                        chatUsers.Add(studentUser!);
                    }

                    var classConversation = new ChatConversation()
                    {
                        ConversationName = newClass.ClassName,
                        ClassId = newClass.ClassId,
                        CreatedAt = DateTime.UtcNow,
                        TeamId = null,
                        Users = chatUsers,
                    };
                    await _unitOfWork.ChatConversationRepo.Create(classConversation);
                    await _unitOfWork.SaveChangesAsync();
                }
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = $"Succesfully imported {request.Classes.Count} classes.";
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
        protected override async Task ValidateRequest(List<OperationError> errors, ImportClassCommand request)
        {
            if (!request.Classes.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.Classes)}",
                    Message = $"There are no classes provided."
                });

                return;
            }

            var lecturerList = await _unitOfWork.LecturerRepo.GetAll();
            var subjectList = await _unitOfWork.SubjectRepo.GetAll();
            var studentList = await _unitOfWork.StudentRepo.GetAll();
            var semesterList = await _unitOfWork.SemesterRepo.GetAll();

            // Validate each class insert request
            for (int index = 0; index < request.Classes.Count; index++)
            {
                var classDto = request.Classes[index];

                // Check duplicates in the request list
                var duplicatedClassIndex = request.Classes.FindIndex(x =>
                    // Only compare different element
                    x != classDto &&
                    x.ClassName.Equals(classDto.ClassName, StringComparison.OrdinalIgnoreCase) &&
                    x.SubjectCode.Equals(classDto.SubjectCode, StringComparison.OrdinalIgnoreCase) &&
                    x.SemesterCode.Equals(classDto.SemesterCode, StringComparison.OrdinalIgnoreCase));
                if (duplicatedClassIndex != -1 && duplicatedClassIndex < index)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"Classes[{index}]",
                        Message = $"Entry 'Classes[{index}]' is a duplicated class info input of 'Classes[{duplicatedClassIndex}]'."
                    });
                }

                // Validate SubjectCode
                var subject = subjectList
                   .FirstOrDefault(x => x.SubjectCode == classDto.SubjectCode);
                if (subject == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"Classes[{index}].{nameof(classDto.SubjectCode)}",
                        Message = $"There is no subject with SubjectCode '{classDto.SubjectCode}'."
                    });
                }

                // Validate SemesterCode
                var semester = semesterList
                   .FirstOrDefault(x => x.SemesterCode == classDto.SemesterCode);
                if (semester == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"Classes[{index}].{nameof(classDto.SemesterCode)}",
                        Message = $"There is no semester with SemesterCode '{classDto.SemesterCode}'."
                    });
                }

                // Check duplicated class
                if (semester != null && subject != null)
                {
                    // Can not create duplicated classes (Same Name, Semester, Subject)
                    var duplicatedClass = await _unitOfWork.ClassRepo.GetDuplicatedClass(
                        classDto.ClassName,
                        subject.SubjectId,
                        semester.SemesterId
                    );
                    if (duplicatedClass != null)
                    {
                        var error = new OperationError()
                        {
                            Field = $"Classes[{index}].{nameof(classDto.ClassName)}",
                            Message = $"There is already a class '{classDto.ClassName}' of subject '{subject.SubjectName}' in semester '{semester.SemesterName}'."
                        };
                        errors.Add(error);
                    }
                }

                // Validate LecturerCode
                var lecturer = lecturerList
                    .FirstOrDefault(x => x.LecturerCode == classDto.LecturerCode);
                if (lecturer == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"Classes[{index}].{nameof(classDto.LecturerCode)}",
                        Message = $"There is no Lecturer with LecturerCode '{classDto.LecturerCode}'."
                    });
                }

                // Validate Students' StudentCode
                if (!classDto.StudentCodes.Any())
                {
                    // Class doesn't have any Student Codes
                    errors.Add(new OperationError()
                    {
                        Field = $"Classes[{index}].{nameof(classDto.StudentCodes)}",
                        Message = $"There are no students in class '{classDto.ClassName}'."
                    });
                }
                else
                {
                    var validCodes = studentList.Select(x => x.StudentCode).ToList();

                    // Get invalid student codes
                    var invalidCodes = classDto.StudentCodes
                        .Where(x => !validCodes.Contains(x))
                        .ToList();
                    if (invalidCodes.Any())
                    {
                        errors.Add(new OperationError()
                        {
                            Field = $"Classes[{index}].{nameof(classDto.StudentCodes)}",
                            Message = $"There were invalid student codes: {string.Join(", ", invalidCodes)}"
                        });
                    }

                    // Check existing students
                    var foundCodes = validCodes.Where(x => classDto.StudentCodes.Contains(x));
                    if (foundCodes.Count() != classDto.StudentCodes.Count)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = $"Classes[{index}].{nameof(classDto.StudentCodes)}",
                            Message = $"Internal database error, input has {classDto.StudentCodes.Count} StudentCode(s) but found {foundCodes.Count()} student(s)."
                        });
                        return;
                    }
                }
            }
        }
    }
}
