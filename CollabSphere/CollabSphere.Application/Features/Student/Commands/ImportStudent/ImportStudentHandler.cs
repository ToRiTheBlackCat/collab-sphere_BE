using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Lecturer.Commands;
using CollabSphere.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Student.Commands.ImportStudent
{
    public class ImportStudentHandler : CommandHandler<ImportStudentCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly ILogger<ImportStudentHandler> _logger;
        private int _parsedYob;


        public ImportStudentHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure,
                                    ILogger<ImportStudentHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(ImportStudentCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            StringBuilder message = new StringBuilder();
            int errCnt = 0;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                foreach (var student in request.StudentList)
                {
                    //Find existed user
                    var foundUser = await _unitOfWork.UserRepo.GetOneByEmail(student.Email ?? "");
                    //If exist user with that email
                    if (foundUser != null)
                    {
                        message.Append($"One Student already existed with email: {student.Email}, cannot create that student! ");
                        errCnt++;
                        continue;
                    }

                    //Check if duplicated studentcode
                    var foundStucode = await _unitOfWork.UserRepo.GetStudentByStudentCodeAsync(student.StudentCode);
                    if (foundStucode != null)
                    {
                        message.Append($"One Student already existed with student code: {student.StudentCode}, cannot create that student! ");
                        errCnt++;
                        continue;
                    }

                    //Hash password
                    var hashedPassword = SHA256Encoding.ComputeSHA256Hash(student.Password + _configure["SecretString"]);

                    //Insert new user
                    var newUser = new Domain.Entities.User
                    {
                        Email = student.Email,
                        Password = hashedPassword,
                        RoleId = RoleConstants.STUDENT,
                        IsTeacher = false,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true,
                    };
                    await _unitOfWork.UserRepo.InsertUser(newUser);
                    await _unitOfWork.SaveChangesAsync();

                    //Insert new Student
                    var newStudent = new Domain.Entities.Student
                    {
                        StudentId = newUser.UId,
                        Fullname = student.FullName,
                        Address = student.Address,
                        PhoneNumber = student.PhoneNumber,
                        Yob = _parsedYob,
                        AvatarImg = "",
                        School = student.School,
                        StudentCode = student.StudentCode,
                        Major = student.Major,
                    };
                    await _unitOfWork.StudentRepo.InsertStudent(newStudent);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                }
                result.IsSuccess = true;
                message.Append($"Succesfully imported {request.StudentList.Count - errCnt} students.");
                result.Message = message.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when create new lecturer account");
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());

                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, ImportStudentCommand request)
        {
            if (!request.StudentList.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.StudentList)}",
                    Message = $"There are not any students in file"
                });

                return;
            }
            for (int i = 0; i < request.StudentList.Count; i++)
            {
                //Check duplicated studentcode in request list
                var studentCodeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var student = request.StudentList[i];

                // Check for duplicated lecturer code inside the import file
                if (!string.IsNullOrEmpty(student.StudentCode))
                {
                    if (!studentCodeSet.Add(student.StudentCode.ToLower().Trim()))
                    {
                        errors.Add(new OperationError()
                        {
                            Field = $"StudentList[{i}].StudentCode",
                            Message = $"Duplicate student code '{student.StudentCode}' found within import list."
                        });
                    }
                }

                //Validate Email format
                var emailFormat = @"^[\x00-\x7F]+@[A-Za-z0-9\p{L}.-]+\.\p{L}+$";

                if (!Regex.IsMatch(request.StudentList[i].Email ?? "", emailFormat))
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"StudentList[{i}].{request.StudentList[i].Email}",
                        Message = $"There is not a valid email format '{request.StudentList[i].Email}'."
                    });
                }

                //Validate PhoneNumber
                var phoneFormat = @"^\d{8,15}$";

                if (!Regex.IsMatch(request.StudentList[i].PhoneNumber ?? "", phoneFormat))
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"LecturerList[{i}].{request.StudentList[i].PhoneNumber}",
                        Message = $"There is not a valid phone number format '{request.StudentList[i].PhoneNumber}'."
                    });
                }

                //Validate Yob
                var isValideYOB = ValidateYearOfBirth(request.StudentList[i].Yob);
                if (!isValideYOB)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"StudentList[{i}].{request.StudentList[i].Yob}",
                        Message = $"There is not a valid year of birth format '{request.StudentList[i].Yob}'."
                    });
                }

                //Validate duplicated student code
                var foundStuCode = await _unitOfWork.UserRepo.GetStudentByStudentCodeAsync(request.StudentList[i].StudentCode);
                if (foundStuCode != null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "StudentCode",
                        Message = $"There is a duplicated student code : {request.StudentList[i].StudentCode}. Try another student code create student"
                    });
                }
                //Validate duplicated student email
                var foundEmail = await _unitOfWork.UserRepo.GetOneByEmail(request.StudentList[i].Email);
                if (foundEmail != null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "Email",
                        Message = $"There is a duplicated student email : {request.StudentList[i].Email}. Try another student email create student"
                    });
                }
            }
        }

        private bool ValidateYearOfBirth(string? yob)
        {
            if (string.IsNullOrEmpty(yob))
                return false;

            if (!int.TryParse(yob, out int year))
                return false;

            int currentYear = DateTime.UtcNow.Year;

            //Set to parsed YOB
            _parsedYob = year;
            return year >= 1900 && year <= currentYear;
        }
    }
}
