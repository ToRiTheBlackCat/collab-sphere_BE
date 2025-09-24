using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Cache;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.OTP
{
    public class SendOTPHandler : IRequestHandler<SendOTPCommand, (bool, string)>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly EmailSender _emailSender;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SendOTPHandler> _logger;

        private static string SUCCESS = "Send OTP successfully";
        private static string FAIL = "Send OTP fail";
        public SendOTPHandler(IUnitOfWork unitOfWork,
                              IConfiguration configure,
                              IMemoryCache cache,
                              ILogger<SendOTPHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _emailSender = new EmailSender(_configure);
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(bool, string)> Handle(SendOTPCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Create OTP code and send to email
                var optCode = _emailSender.SendOTPToEmail(request.Email);
                if (optCode == null)
                {
                    return (false, FAIL);
                }
                //Save OTP code to cache
                var tempSignUpOTPCache = new TempSignUpOTPCache
                {
                    OtpCode = optCode,
                    ExpireAt = DateTime.UtcNow.AddMinutes(15)
                };
                _cache.Set(request.Email, tempSignUpOTPCache, TimeSpan.FromMinutes(15));

                return (true, SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when send OTP to email: {Email}", request.Email);
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());

                return (false, FAIL);
            }
        }
    }
}
