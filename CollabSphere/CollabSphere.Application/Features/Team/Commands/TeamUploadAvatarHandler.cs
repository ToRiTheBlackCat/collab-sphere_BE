using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands
{
    public class TeamUploadAvatarHandler : CommandHandler<TeamUploadAvatarCommand>
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TeamUploadAvatarHandler> _logger;
        private readonly CloudinaryService _cloudinaryService;


        public TeamUploadAvatarHandler(IUnitOfWork unitOfWork,
                                 ILogger<TeamUploadAvatarHandler> logger,
                                 CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }


        protected override async Task<CommandResult> HandleCommand(TeamUploadAvatarCommand request, CancellationToken cancellationToken)
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

                var foundTeam = _unitOfWork.TeamRepo.GetById(request.TeamId).Result;
                if (foundTeam != null)
                {
                    //Upload image and receive publicId for storage in DB
                    var publicId = await _cloudinaryService.UploadImageAsync(request.ImageFile, request.Folder);
                    if (publicId == null)
                    {
                        result.Message = "Fail to upload image to Cloudianry";
                        return result;
                    }

                    //Store old image
                    var oldImage = foundTeam.TeamImage;

                    //Save to DB
                    foundTeam.TeamImage = publicId;
                    _unitOfWork.TeamRepo.Update(foundTeam);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    //Delete old image in Cloudinary
                    var isDelete = await _cloudinaryService.DeleteImageAsync(oldImage);
                    if (!isDelete)
                    {
                        result.Message = "Fail to delete old image in Cloudinary";
                    }

                    result.IsSuccess = true;
                    result.Message = "Upload team avatar successfully.";
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Exception when upload team avatar");
                result.Message = ex.Message;

            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, TeamUploadAvatarCommand request)
        {
            //Check existed team
            var foundTeam = _unitOfWork.TeamRepo.GetById(request.TeamId).Result;
            if (foundTeam == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.TeamId),
                    Message = $"Not found any team with that Id: {request.TeamId}"
                });
                return;
            }

            //Check requester permission
            var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.RequesterId);

            if(foundUser == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.RequesterId),
                    Message = $"Not found any user with that Id: {request.RequesterId}"
                });
                return;
            }

            //If lecturer
            if (foundUser.IsTeacher)
            {
                if (foundUser.UId != foundTeam.LecturerId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.RequesterId),
                        Message = $"You don't have permission to change avatar of this team"
                    });
                    return;
                }
            }
            //If student (leader)
            else
            {
                if (foundUser.UId != foundTeam.LeaderId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.RequesterId),
                        Message = $"You don't have permission to change avatar of this team. Only leader can change avatar of team"
                    });
                    return;
                }
            }
        }
    }
}
