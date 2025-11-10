using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CollabSphere.Application.Features.VideoRecord.Commands.UploadRecord
{
    public class UploadRecordHandler : CommandHandler<UploadRecordCommand>
    {
        private readonly GgDriveVideoService _ggDriveService;
        public UploadRecordHandler(GgDriveVideoService ggDriveService)
        {
            _ggDriveService = ggDriveService;
        }

        protected override async Task<CommandResult> HandleCommand(UploadRecordCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var videoUrl = await _ggDriveService.UploadFileAsync(request.VideoFile);

                if (videoUrl != null)
                {
                    result.IsSuccess = true;
                    result.Message = videoUrl;
                }
                else
                {
                    result.Message = "Upload video failed";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UploadRecordCommand request)
        {
            if (request.VideoFile == null || request.VideoFile.Length == 0)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.VideoFile),
                    Message = $"No file upload"
                });
                return;
            }

            if (request.VideoFile.Length > 1024 * 1024 * 1024)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.VideoFile),
                    Message = "File size cannot exceed 1 GB"
                });
                return;
            }
        }
    }
}
