using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operations
                foreach (var classDto in request.Classes)
                {
                    // Find subject
                    var subject = subjectList.First(x => x.SubjectCode == classDto.SubjectCode);

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
                        EnrolKey = classDto.EnrolKey,
                        SubjectId = subject.SubjectId,
                        Subject = subject,
                        LecturerId = lecturer.LecturerId,
                        LecturerName = lecturer.Fullname,
                        Lecturer = lecturer,
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

            // Validate each class insert request
            for (int index = 0; index < request.Classes.Count; index++)
            {
                var classDto = request.Classes[index];

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

                    return;
                }
                else
                {
                    // Check student codes in class
                    var existStudents = studentList
                        .Where(x => classDto.StudentCodes.Contains(x.StudentCode));

                    if (existStudents.Count() != classDto.StudentCodes.Count)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = $"Classes[{index}].{nameof(classDto.StudentCodes)}",
                            Message = $"Some student codes in class '{classDto.ClassName}' are invalid."
                        });

                        return;
                    }
                }
            }
        }
    }
}
