using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Lecturer.Commands;
using CollabSphere.Application.Features.Student.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Commands
{
    public class HeadDepart_StaffSignUpSignUpHandler : IRequestHandler<HeadDepart_StaffSignUpCommand, (bool, string)>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly ILogger<StudentSignUpHandler> _logger;

        private static string SUCCESS = "Create new staff or head department successfully";
        private static string EXCEPTION = "Exception when create new staff or head department";
        private static string EXISTED = "Already exist staff or head department with that email. Try other email to create account";

        public HeadDepart_StaffSignUpSignUpHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure,
                                    ILogger<StudentSignUpHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _logger = logger;
        }

        public async Task<(bool, string)> Handle(HeadDepart_StaffSignUpCommand request, CancellationToken cancellationToken)
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
                    RoleId = request.Dto.isStaff ? RoleConstants.STAFF : RoleConstants.HEAD_DEPARTMENT,
                    IsTeacher = false,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                await _unitOfWork.UserRepo.InsertUser(newUser);
                await _unitOfWork.CommitTransactionAsync();

                return (true, SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when create new staff or academic account with email: {Email}", request.Dto.Email);
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());
                await _unitOfWork.RollbackTransactionAsync();

                return (false, EXCEPTION);
            }
        }
    }
}
