using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic; // Added for List<string>
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public class GgDriveSettings
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class GgDriveVideoService
    {
        private readonly DriveService _driveService;

        public GgDriveVideoService(IOptions<GgDriveSettings> config)
        {
            var settings = config.Value;

            if (string.IsNullOrEmpty(settings.ClientId))
                throw new ArgumentNullException(nameof(settings.ClientId));
            if (string.IsNullOrEmpty(settings.ClientSecret))
                throw new ArgumentNullException(nameof(settings.ClientSecret));
            if (string.IsNullOrEmpty(settings.RefreshToken))
                throw new ArgumentNullException(nameof(settings.RefreshToken));

            // Set up the ClientSecrets
            var clientSecrets = new ClientSecrets
            {
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret
            };

            // Set up the TokenResponse with Refresh Token
            var tokenResponse = new TokenResponse
            {
                RefreshToken = settings.RefreshToken
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = new[] { DriveService.Scope.DriveFile }
            });

            var credential = new UserCredential(flow, "user", tokenResponse);

            // Create the Drive service
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = settings.ApplicationName
            });
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = file.FileName,
                Parents = new List<string> { "1TN58i6vqPdBOwFQq5uwWjVHRcusVp-Mj" }
            };

            using var stream = file.OpenReadStream();
            var request = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
            request.Fields = "id";
            try
            {
                var uploadProgress = await request.UploadAsync();

                if (uploadProgress.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    throw new Exception($"Google Drive Upload Failed: {uploadProgress.Exception.Message}", uploadProgress.Exception);
                }

                var uploadedFile = request.ResponseBody;

                if (uploadedFile == null)
                {
                    throw new Exception("File upload failed, Google Drive did not return a file ID.");
                }


                // Set the permission
                var permission = new Google.Apis.Drive.v3.Data.Permission
                {
                    Type = "anyone",
                    Role = "reader"
                };

                await _driveService.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

                // Return the link
                return $"https://drive.google.com/uc?id={uploadedFile.Id}&export=download";
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}