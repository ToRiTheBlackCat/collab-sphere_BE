﻿using CloudinaryDotNet;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Student.Commands;
using CollabSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Commands
{
    public class ImportLecturerHandler : CommandHandler<ImportLecturerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly ILogger<ImportLecturerHandler> _logger;
        private int _parsedYob;
        public ImportLecturerHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure,
                                    ILogger<ImportLecturerHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(ImportLecturerCommand request, CancellationToken cancellationToken)
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

                foreach (var lecturer in request.LecturerList)
                {
                    //Find existed user
                    var foundUser = await _unitOfWork.UserRepo.GetOneByEmail(lecturer.Email ?? "");
                    //If exist user with that email
                    if (foundUser != null)
                    {
                        continue;
                    }

                    //Hash password
                    var hashedPassword = SHA256Encoding.ComputeSHA256Hash(lecturer.Password + _configure["SecretString"]);

                    //Insert new user
                    var newUser = new Domain.Entities.User
                    {
                        Email = lecturer.Email,
                        Password = hashedPassword,
                        RoleId = RoleConstants.LECTURER,
                        IsTeacher = true,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true,
                    };
                    await _unitOfWork.UserRepo.InsertUser(newUser);
                    await _unitOfWork.SaveChangesAsync();

                    //Create new Lecturer
                    var newLecturer = new Domain.Entities.Lecturer
                    {
                        LecturerId = newUser.UId,
                        Fullname = lecturer.FullName,
                        Address = lecturer.Address,
                        PhoneNumber = lecturer.PhoneNumber,
                        Yob = _parsedYob,
                        AvatarImg = "",
                        School = lecturer.School,
                        LecturerCode = lecturer.LecturerCode,
                        Major = lecturer.Major,
                    };
                    await _unitOfWork.LecturerRepo.InsertLecturer(newLecturer);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Succesfully imported {request.LecturerList.Count} lecturers.";
                }
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

        protected override async Task ValidateRequest(List<OperationError> errors, ImportLecturerCommand request)
        {
            if (!request.LecturerList.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.LecturerList)}",
                    Message = $"There are not any lecturers in file"
                });

                return;
            }
            for (int i = 0; i < request.LecturerList.Count; i++)
            {
                //Validate Email format
                var emailFormat = @"^[\x00-\x7F]+@[A-Za-z0-9\p{L}.-]+\.\p{L}+$";

                if (!Regex.IsMatch(request.LecturerList[i].Email ?? "", emailFormat))
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"LecturerList[{i}].{(request.LecturerList[i].Email)}",
                        Message = $"There is not a valid email format '{request.LecturerList[i].Email}'."
                    });
                }

                //Validate PhoneNumber
                var phoneFormat = @"^\d{8,15}$";

                if (!Regex.IsMatch(request.LecturerList[i].PhoneNumber ?? "", phoneFormat))
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"LecturerList[{i}].{(request.LecturerList[i].PhoneNumber)}",
                        Message = $"There is not a valid phone number format '{request.LecturerList[i].PhoneNumber}'."
                    });
                }

                //Validate Yob
                var isValideYOB = ValidateYearOfBirth(request.LecturerList[i].Yob);
                if (!isValideYOB)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"LecturerList[{i}].{(request.LecturerList[i].Yob)}",
                        Message = $"There is not a valid year of birth format '{request.LecturerList[i].Yob}'."
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
