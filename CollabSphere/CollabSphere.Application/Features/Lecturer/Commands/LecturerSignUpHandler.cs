using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Student;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Commands
{
    public class LecturerSignUpHandler : IRequestHandler<LecturerSignUpCommand, (bool, string)>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly ILogger<StudentSignUpHandler> _logger;

        private static string SUCCESS = "Create new lecturer successfully";
        private static string EXCEPTION = "Exception when create new lecturer";
        private static string EXISTED = "Already exist lecturer with that email. Try other email to sign up";

        public LecturerSignUpHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure,
                                    ILogger<StudentSignUpHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _logger = logger;
        }

        public async Task<(bool, string)> Handle(LecturerSignUpCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Check existed user
                var foundUser = await _unitOfWork.UserRepo.GetOneByEmail(request.Dto.Email);
                //If exist user with that email
                if (foundUser != null)
                {
                    return (false, EXISTED);
                }

                var hashedPassword = SHA256Encoding.ComputeSHA256Hash(request.Dto.Password + _configure["SecretString"]);

                //Create new user
                var newUser = new Domain.Entities.User
                {
                    Email = request.Dto.Email,
                    Password = hashedPassword,
                    RoleId = RoleConstants.LECTURER,
                    IsTeacher = false,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.UserRepo.InsertUser(newUser);
                await _unitOfWork.SaveChangesAsync();

                //Create new Lecturer
                var newLecturer = new Domain.Entities.Lecturer
                {
                    LecturerId = newUser.UId,
                    Fullname = request.Dto.FullName,
                    Address = request.Dto.Address,
                    PhoneNumber = request.Dto.PhoneNumber,
                    Yob = request.Dto.Yob,
                    AvatarImg = "",
                    School = request.Dto.School,
                    LecturerCode = request.Dto.LecturerCode,
                    Major = request.Dto.Major,
                };

                await _unitOfWork.LecturerRepo.InsertLecturer(newLecturer);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (true, SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when create new lecturer account with email: {Email}", request.Dto.Email);
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());
                await _unitOfWork.RollbackTransactionAsync();

                return (false, EXCEPTION);
            }
        }
    }
}
