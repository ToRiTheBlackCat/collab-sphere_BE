using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Auth.Commands;
using CollabSphere.Application.Features.Team.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Commands.UserUpdateProfile
{
    public class UpdateUserProfileHandler : CommandHandler<UpdateUserProfileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateUserProfileHandler> _logger;
        private readonly IConfiguration _configure;
        private readonly JWTAuthentication _jwtAuth;


        public UpdateUserProfileHandler(IUnitOfWork unitOfWork,
                            IConfiguration configure,
                            JWTAuthentication jwtAuth,
                            ILogger<UpdateUserProfileHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _jwtAuth = jwtAuth;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateUserProfileCommand request, CancellationToken cancellationToken)
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

                //Find existed user
                var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId);

                #region Update Profile of found User
                if (foundUser != null)
                {
                    //Update password if changing
                    if (!string.IsNullOrEmpty(request.NewPassword))
                    {
                        var hashedPassword = SHA256Encoding.ComputeSHA256Hash(request.NewPassword + _configure["SecretString"]);
                        foundUser.Password = hashedPassword;
                    }

                    //Update IsActive
                    foundUser.IsActive = request.IsActive;

                    #region Update if Lecturer
                    if (request.IsTeacher)
                    {
                        //Update Fullname 
                        if (!string.IsNullOrEmpty(request.FullName))
                        {
                            foundUser.Lecturer.Fullname = request.FullName.Trim();
                        }

                        //Update Address
                        if (!string.IsNullOrEmpty(request.Address))
                        {
                            foundUser.Lecturer.Address = request.Address.Trim();
                        }

                        //Update Phone Number
                        if (!string.IsNullOrEmpty(request.PhoneNumber))
                        {
                            foundUser.Lecturer.PhoneNumber = request.PhoneNumber;
                        }

                        //Update YOB
                        if (request.Yob != 0)
                        {
                            foundUser.Lecturer.Yob = request.Yob;
                        }

                        //Update School
                        if (!string.IsNullOrEmpty(request.School))
                        {
                            foundUser.Lecturer.School = request.School.Trim();
                        }

                        //Update LecturerCode
                        if (!string.IsNullOrEmpty(request.Code))
                        {
                            foundUser.Lecturer.LecturerCode = request.Code.Trim();
                        }

                        //Update Major
                        if (!string.IsNullOrEmpty(request.Major))
                        {
                            foundUser.Lecturer.Major = request.Major.Trim();
                        }
                    }
                    #endregion

                    #region Update if Student
                    else if (!request.IsTeacher)
                    {
                        //Update Fullname 
                        if (!string.IsNullOrEmpty(request.FullName))
                        {
                            foundUser.Student.Fullname = request.FullName.Trim();
                        }

                        //Update Address
                        if (!string.IsNullOrEmpty(request.Address))
                        {
                            foundUser.Student.Address = request.Address.Trim();
                        }

                        //Update Phone Number
                        if (!string.IsNullOrEmpty(request.PhoneNumber))
                        {
                            foundUser.Student.PhoneNumber = request.PhoneNumber;

                        }

                        //Update YOB
                        if (request.Yob != 0)
                        {
                            foundUser.Student.Yob = request.Yob;
                        }

                        //Update School
                        if (!string.IsNullOrEmpty(request.School))
                        {
                            foundUser.Student.School = request.School.Trim();
                        }

                        //Update LecturerCode
                        if (!string.IsNullOrEmpty(request.Code))
                        {
                            foundUser.Student.StudentCode = request.Code.Trim();
                        }

                        //Update Major
                        if (!string.IsNullOrEmpty(request.Major))
                        {
                            foundUser.Student.Major = request.Major.Trim();
                        }
                    }
                    #endregion

                    _unitOfWork.UserRepo.UpdateUser(foundUser);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Update user profile sucessfully";
                    _logger.LogInformation($"Update profile of user with ID: {request.UserId} sucessfully");
                }
                #endregion
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                _logger.LogError(ex, "Error occurred while creating team.");
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateUserProfileCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.STAFF, RoleConstants.STUDENT, RoleConstants.LECTURER };

            //Check existed user
            var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId);
            if (foundUser == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.UserId),
                    Message = $"Cannot find any user with that Id: {request.UserId}"
                });
                return;
            }
            //Check if role is valid to update profile
            if (bypassRoles.Contains(request.RequesterRole))
            {
                //Check update profile permission of requester
                if (request.RequesterRole == RoleConstants.STUDENT && request.RequesterId != foundUser.UId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.RequesterId),
                        Message = $"Your are not allowed to update this user profile."
                    });
                }
                else if (request.RequesterRole == RoleConstants.LECTURER && request.RequesterId != foundUser.UId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.RequesterId),
                        Message = $"Your are not allowed to update this user profile."
                    });
                }

                if (!string.IsNullOrEmpty(request.OldPassword))
                {
                    //Check match old password
                    var oldPassword = SHA256Encoding.ComputeSHA256Hash(request.OldPassword + _configure["SecretString"]);
                    if (foundUser.Password != oldPassword)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.OldPassword),
                            Message = $"Not match with found old password. Try again!"
                        });
                    }
                }
            }
            else
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.RequesterRole),
                    Message = $"Your are not allowed to update this user profile"
                });
                return;
            }
        }
    }
}
