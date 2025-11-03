using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.User;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CollabSphere.Application.Features.Auth.Commands
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly ILogger<LoginHandler> _logger;
        private readonly CloudinaryService _cloudinaryService;

        private readonly JWTAuthentication _jwtAuth;

        int expireDay = 7;

        public LoginHandler(IUnitOfWork unitOfWork,
                            IConfiguration configure,
                            JWTAuthentication jwtAuth,
                            ILogger<LoginHandler> logger,
                            CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _jwtAuth = jwtAuth;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<LoginResponseDTO> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var responseDto = new LoginResponseDTO();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Hash the password
                var hashedPassword = SHA256Encoding.ComputeSHA256Hash(request.Dto.Password + _configure["SecretString"]);

                //Find existed user
                var foundUser = await _unitOfWork.UserRepo.GetOneByEmailAndPassword(request.Dto.Email, hashedPassword);

                //If not found
                if (foundUser == null)
                {
                    _logger.LogInformation("Not found any user with Email: {Enail}", request.Dto.Email);

                    responseDto.IsAuthenticated = false;
                    return responseDto;
                }
                else
                {
                    //If admin
                    if(foundUser.RoleId == RoleConstants.ADMIN)
                    {
                        responseDto.IsAuthenticated = true;
                        responseDto.FullName = "COLLABSPHERE_ADMIN";

                        return responseDto;
                    }


                    //Generate both access & refresh token
                    var (accessToken, refreshToken) = await _jwtAuth.GenerateToken(foundUser);

                    //Save refresh token to DB
                    foundUser.RefreshToken = refreshToken;
                    foundUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(expireDay);
                    _unitOfWork.UserRepo.UpdateUser(foundUser);

                    //Commit transaction
                    await _unitOfWork.CommitTransactionAsync();

                    //Set data to response model
                    responseDto.IsAuthenticated = true;
                    responseDto.UserId = foundUser.UId.ToString().Trim();
                    responseDto.RoleId = foundUser.RoleId;
                    responseDto.RoleName = foundUser.Role.RoleName.Trim();
                    responseDto.AccessToken = accessToken;
                    responseDto.RefreshToken = refreshToken;
                    responseDto.RefreshTokenExpiryTime = foundUser.RefreshTokenExpiryTime;

                    var fullName = "";
                    var avatar = "";
                    if (foundUser.IsTeacher)
                    {
                        fullName = foundUser.Lecturer.Fullname;
                        avatar = await _cloudinaryService.GetImageUrl(foundUser.Lecturer.AvatarImg);
                    }
                    else
                    {
                        fullName = foundUser.Student?.Fullname;
                        avatar = await _cloudinaryService.GetImageUrl(foundUser.Student.AvatarImg);
                    }

                    responseDto.FullName = fullName;
                    responseDto.Avatar = avatar;

                    return responseDto;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when update refresh token and expiry time to DB");
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());
                //Roll back the transaction
                await _unitOfWork.RollbackTransactionAsync();

                responseDto.IsAuthenticated = false;
                return responseDto;
            }
        }
    }
}
