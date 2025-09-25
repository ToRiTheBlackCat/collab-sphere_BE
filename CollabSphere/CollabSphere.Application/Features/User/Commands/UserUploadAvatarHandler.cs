using CloudinaryDotNet;
using CloudinaryDotNet.Core;
using CollabSphere.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Commands
{
    public class UserUploadAvatarHandler : IRequestHandler<UserUploadAvatarCommand, (bool, string)>
    {

        private readonly CloudinaryService _cloudinaryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserUploadAvatarHandler> _logger;

        private static string SUCCESS = "Avatar uploaded successfully!";
        private static string FAIL = "Avatar uploaded fail!";
        private static string EXCEPTION = "Exception when upload avatar";
        private static string NOTFOUND = "Not found any user with that userIdc";


        public UserUploadAvatarHandler(IUnitOfWork unitOfWork,
                                       CloudinaryService cloudinaryService,
                                       ILogger<UserUploadAvatarHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<(bool, string)> Handle(UserUploadAvatarCommand request, CancellationToken cancellationToken)
        {
            var isTeacher = request.IsTeacher;
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find user
                var foundUser = await _unitOfWork.UserRepo.GetOneByIdWithIncludeAsync(request.UserId, "UId", u => u.Student, u => u.Lecturer);

                if (foundUser == null)
                {
                    _logger.LogError("Not found any user with userId: {UserId}", request.UserId);
                    return (false, NOTFOUND);
                }

                //Upload image and receive publicId for storage in DB
                var publicId = await _cloudinaryService.UploadImageAsync(request.ImageFile, "avatars");
                if (string.IsNullOrEmpty(publicId))
                {
                    _logger.LogError("Fail to upload image to Cloudianry");
                    return (false, FAIL);
                }

                /*Check if is lecturer or student
                 Then save the publicId into avatarImg field*/
                if (isTeacher)
                {
                    var updateLecturer = foundUser.Lecturer;
                    updateLecturer.AvatarImg = publicId;

                    _unitOfWork.LecturerRepo.UpdateLecturer(updateLecturer);
                }
                else
                {
                    var updateStudent = foundUser.Student;
                    updateStudent.AvatarImg = publicId;

                    _unitOfWork.StudentRepo.UpdateStudent(updateStudent);
                }

                await _unitOfWork.CommitTransactionAsync();

                return (true, SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when upload avatar image for user with id: {Id}", request.UserId);
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());
                await _unitOfWork.RollbackTransactionAsync();

                return (false, EXCEPTION);
            }
        }
    }
}
