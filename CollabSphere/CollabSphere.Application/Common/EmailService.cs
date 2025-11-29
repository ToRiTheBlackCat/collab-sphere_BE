using CollabSphere.Application.Features.Admin.Queries;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public class EmailSettings
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
    public class EmailService
    {
        private readonly GmailService _gmailService;

        public EmailService(IOptions<EmailSettings> config)
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
            _gmailService = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = settings.ApplicationName
            });
        }

        public async Task<List<EmailDto>> GetRecentEmailsAsync(int count)
        {
            var result = new List<EmailDto>();

            // 1. Lấy danh sách ID các email gần nhất
            var listRequest = _gmailService.Users.Messages.List("me");
            listRequest.MaxResults = count;
            // LabelIds = "INBOX" để chỉ lấy trong hộp thư đến (bỏ qua Spam/Trash)
            listRequest.LabelIds = new List<string> { "INBOX" };

            var listResponse = await listRequest.ExecuteAsync();

            if (listResponse.Messages != null && listResponse.Messages.Count > 0)
            {
                // 2. Loop qua từng ID để lấy chi tiết (Subject, From, ...)
                // Lưu ý: Có thể dùng BatchRequest để tối ưu tốc độ nếu lấy nhiều
                foreach (var msgItem in listResponse.Messages)
                {
                    var emailDetail = await GetEmailDetailsAsync(msgItem.Id);
                    if (emailDetail != null)
                    {
                        result.Add(emailDetail);
                    }
                }
            }

            return result;
        }
        private async Task<EmailDto> GetEmailDetailsAsync(string messageId)
        {
            var request = _gmailService.Users.Messages.Get("me", messageId);
            request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
            // Dùng METADATA nhẹ hơn FULL nếu chỉ cần hiển thị danh sách
            // Nếu muốn lấy Body để đọc nội dung thì đổi thành FULL

            var message = await request.ExecuteAsync();

            if (message == null) return null;

            var dto = new EmailDto
            {
                Id = message.Id,
                ThreadId = message.ThreadId,
                Snippet = message.Snippet,
                IsRead = !message.LabelIds.Contains("UNREAD") // Kiểm tra nhãn UNREAD
            };

            // Header nằm trong Payload.Headers
            if (message.Payload?.Headers != null)
            {
                foreach (var header in message.Payload.Headers)
                {
                    if (header.Name == "Subject") dto.Subject = header.Value;
                    if (header.Name == "From") dto.From = header.Value;
                    if (header.Name == "Date") dto.Date = header.Value;
                }
            }

            return dto;
        }
    }

   
}
