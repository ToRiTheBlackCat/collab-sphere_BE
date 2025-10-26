using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Cache;
using CollabSphere.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Commands.SignUpStudent
{
    public class StudentSignUpHandler : IRequestHandler<StudentSignUpCommand, (bool, string)>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configure;
        private readonly ILogger<StudentSignUpHandler> _logger;

        private static string SUCCESS = "Create new student successfully";
        private static string FAIL = "Create new student fail. Not valid OTP code";
        private static string EXCEPTION = "Exception when create new student";
        private static string EXISTED = "Already exist student with that email. Try other email to sign up";
        private static string EXISTEDSTUCODE = "Already exist student with that student code. Try other studentcode to create account";

        public StudentSignUpHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure,
                                    IMemoryCache cache,
                                    ILogger<StudentSignUpHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _cache = cache;
            _logger = logger;
        }

        public async Task<(bool, string)> Handle(StudentSignUpCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Validate OPT code
                if (ValidateOPT(request.Dto.Email, request.Dto.OtpCode))
                {
                    //Check existed user
                    var foundUser = await _unitOfWork.UserRepo.GetOneByEmail(request.Dto.Email);
                    //If exist user with that email
                    if (foundUser != null)
                    {
                        return (false, EXISTED);
                    }

                    //Check duplicated studentcode
                    var foundStucode = await _unitOfWork.UserRepo.GetStudentByStudentCodeAsync(request.Dto.StudentCode);
                    if (foundStucode != null)
                    {
                        return(false, EXISTEDSTUCODE);
                    }

                    var hashedPassword = SHA256Encoding.ComputeSHA256Hash(request.Dto.Password + _configure["SecretString"]);

                    //Create new user
                    var newUser = new Domain.Entities.User
                    {
                        Email = request.Dto.Email,
                        Password = hashedPassword,
                        RoleId = RoleConstants.STUDENT,
                        IsTeacher = false,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.UserRepo.InsertUser(newUser);
                    await _unitOfWork.SaveChangesAsync();

                    //Create new Student account
                    var newStudent = new Domain.Entities.Student
                    {
                        StudentId = newUser.UId,
                        Fullname = request.Dto.FullName,
                        Address = request.Dto.Address,
                        PhoneNumber = request.Dto.PhoneNumber,
                        Yob = request.Dto.Yob ?? 2025,
                        AvatarImg = "",
                        School = request.Dto.School,
                        StudentCode = request.Dto.StudentCode,
                        Major = request.Dto.Major,
                    };

                    await _unitOfWork.StudentRepo.InsertStudent(newStudent);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    //Remove existed OTP in cache
                    RemoveOTP(request.Dto.Email);

                    return (true, SUCCESS);
                }

                return (false, FAIL);
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when create new student account with email: {Email}", request.Dto.Email);
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());
                await _unitOfWork.RollbackTransactionAsync();

                return (false, EXCEPTION);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="inputOtpCode"></param>
        /// <returns></returns>
        private bool ValidateOPT(string email, string inputOtpCode)
        {
            if (_cache.TryGetValue(email, out TempSignUpOTPCache? cacheOtpCode))
            {
                return cacheOtpCode?.ExpireAt >= DateTime.UtcNow && cacheOtpCode?.OtpCode == inputOtpCode;
            }

            return false;
        }

        /// <summary>
        /// Support function using for remove OTPCode in cache
        /// </summary>
        /// <param name="email"> Input email for delete the OTP code</param>
        private void RemoveOTP(string email)
        {
            _cache.Remove(email);
        }
    }
}
