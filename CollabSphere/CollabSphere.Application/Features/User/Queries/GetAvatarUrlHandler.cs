using CollabSphere.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Queries
{

    public record GetAvatarUrlQuery(string publicId) : IRequest<string>;

    public class GetAvatarUrlHandler : IRequestHandler<GetAvatarUrlQuery, string>
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<GetAvatarUrlHandler> _logger;

        public GetAvatarUrlHandler(CloudinaryService cloudinaryService,
                                   ILogger<GetAvatarUrlHandler> logger)
        {
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<string> Handle(GetAvatarUrlQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var imageUrl = await _cloudinaryService.GetImageUrl(request.publicId);
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when getting image url of publicId: {PublicId}", request.publicId);
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());

                return string.Empty;
            }
        }
    }
}
