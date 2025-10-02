using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Token;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Token.Commands
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RefreshTokenHandler> _logger;
        private readonly JWTAuthentication _jwtAuth;

        private static string SUCCESS = "Create new tokens successfully";
        private static string EXCEPTION = "Exception when create new tokens";
        private static string NOTFOUND = "Cannot find any user with that Id";
        private static string NOTMATCH = "Invalid Refresh token! Token not found";

        public RefreshTokenHandler(IUnitOfWork unitOfWork,
                                   ILogger<RefreshTokenHandler> logger,
                                   JWTAuthentication jwtAuth)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jwtAuth = jwtAuth;
        }

        public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var response = new RefreshTokenResponseDto();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find existed user
                var foundUser = await _unitOfWork.UserRepo.GetOneByUserIdAsync(request.Dto.UserId);
                if (foundUser == null)
                {
                    response.Message = NOTFOUND;
                    return response;
                }

                //Check match refresh token in DB
                if (foundUser.RefreshToken != request.Dto.RefreshToken)
                {
                    response.Message = NOTMATCH;
                    return response;
                }

                //Create new tokens
                var (newAccess, newRefresh) = await _jwtAuth.RefreshTokenAsync(foundUser, request.Dto.RefreshToken);

                //Save to DB
                foundUser.RefreshToken = newRefresh;
                _unitOfWork.UserRepo.UpdateUser(foundUser);
                await _unitOfWork.CommitTransactionAsync();

                //Set data to response model
                response.IsSuccess = true;
                response.Message = SUCCESS;
                response.AccessToken = newAccess;
                response.RefreshToken = newRefresh;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(EXCEPTION);
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());

                //Roll back the transaction
                await _unitOfWork.RollbackTransactionAsync();

                response.Message = EXCEPTION;
                return response;
            }
        }
    }
}
