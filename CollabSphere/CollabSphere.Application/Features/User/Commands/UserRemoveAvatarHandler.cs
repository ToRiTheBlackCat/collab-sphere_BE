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
    public class UserRemoveAvatarHandler : IRequestHandler<UserRemoveAvatarCommand, (bool, string)>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserRemoveAvatarHandler> _logger;
        private readonly CloudinaryService _cloudinaryService;

        private static string SUCCESS = "Avatar deleted successfully!";
        private static string FAIL = "Avatar deleted fail!";
        private static string EXCEPTION = "Exception when delete avatar";
        private static string NOTFOUNDUSER = "Not found any user with that userId";
        private static string NOTFOUNDAVATAR = "Not found any avatar url with that publicId";


        public UserRemoveAvatarHandler(IUnitOfWork unitOfWork,
                                       ILogger<UserRemoveAvatarHandler> logger,
                                       CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<(bool, string)> Handle(UserRemoveAvatarCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find user
                var foundUser = await _unitOfWork.UserRepo.GetOneByIdWithIncludeAsync(request.Dto.UserId, "UId", u => u.Student, u => u.Lecturer);
                if (foundUser == null)
                {
                    return (false, NOTFOUNDUSER);
                }

                //Find avatar url
                var avatarUrl = _cloudinaryService.GetImageUrl(request.Dto.PublicId);
                if (avatarUrl == null)
                {
                    return (false, NOTFOUNDAVATAR);
                }

                //Remove avatarImg in DB
                if (foundUser.IsTeacher)
                {
                    var lecturer = foundUser.Lecturer;
                    lecturer.AvatarImg = string.Empty;

                    _unitOfWork.LecturerRepo.UpdateLecturer(lecturer);
                }
                else
                {
                    var student = foundUser.Student;
                    student.AvatarImg = string.Empty;

                    _unitOfWork.StudentRepo.UpdateStudent(student);
                }
                await _unitOfWork.CommitTransactionAsync();

                //Delete avatar image in Cloudinary
                var isDelete = await _cloudinaryService.DeleteImageAsync(request.Dto.PublicId);

                return isDelete
                    ? (true, SUCCESS)
                    : (false, FAIL);
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when delete avatar image for user with id: {Id}", request.Dto.UserId);
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());
                await _unitOfWork.RollbackTransactionAsync();

                return (false, EXCEPTION);
            }
        }
    }
}
