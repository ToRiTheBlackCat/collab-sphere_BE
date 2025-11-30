using CollabSphere.Application.Features.Admin.Queries.AdminGetEmails;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
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
                    var emailDetail = await GetEmailDetailsForListAsync(msgItem.Id);
                    if (emailDetail != null)
                    {
                        result.Add(emailDetail);
                    }
                }
            }

            return result;
        }
        private async Task<EmailDto> GetEmailDetailsForListAsync(string messageId)
        {
            var request = _gmailService.Users.Messages.Get("me", messageId);
            request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Metadata;
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

        public async Task<EmailDto?> GetEmailDetailsAsync(string messageId)
        {
            try
            {
                var request = _gmailService.Users.Messages.Get("me", messageId);
                request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                // Dùng METADATA nhẹ hơn FULL nếu chỉ cần hiển thị danh sách
                // Nếu muốn lấy Body để đọc nội dung thì đổi thành FULL

                var messageResponse = await request.ExecuteAsync();
                if (messageResponse == null) return null;

                if (messageResponse.LabelIds != null && messageResponse.LabelIds.Contains("UNREAD"))
                {
                    var mods = new ModifyMessageRequest
                    {
                        RemoveLabelIds = new List<string> { "UNREAD" }
                    };
                    // Call modify to boost the performance
                    _ = _gmailService.Users.Messages.Modify(mods, "me", messageId).ExecuteAsync();
                }

                var dto = new EmailDto
                {
                    Id = messageResponse.Id,
                    ThreadId = messageResponse.ThreadId,
                    Snippet = messageResponse.Snippet,
                    IsRead = true // Kiểm tra nhãn UNREAD
                };

                // Header nằm trong Payload.Headers
                if (messageResponse.Payload?.Headers != null)
                {
                    foreach (var header in messageResponse.Payload.Headers)
                    {
                        if (header.Name == "Subject") dto.Subject = header.Value;
                        if (header.Name == "From") dto.From = header.Value;
                        if (header.Name == "Date") dto.Date = header.Value;
                    }
                }
                dto.Body = GetMessageBody(messageResponse.Payload);

                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting email details: {ex.Message}");
                return null;
            }
        }

        private string GetMessageBody(MessagePart payload)
        {
            if (payload == null) return "";

            string body = "";

            // Trường hợp 1: Nội dung nằm ngay ở Body.Data (thường là mail đơn giản)
            if (payload.Body != null && !string.IsNullOrEmpty(payload.Body.Data))
            {
                return DecodeBase64Url(payload.Body.Data);
            }

            // Trường hợp 2: Nội dung nằm trong các Parts (Multipart/Alternative)
            if (payload.Parts != null && payload.Parts.Count > 0)
            {
                // Ưu tiên tìm part có mimeType là "text/html"
                var htmlPart = payload.Parts.FirstOrDefault(p => p.MimeType == "text/html");
                if (htmlPart != null)
                {
                    return GetMessageBody(htmlPart); // Đệ quy
                }

                // Nếu không có HTML, lấy "text/plain"
                var textPart = payload.Parts.FirstOrDefault(p => p.MimeType == "text/plain");
                if (textPart != null)
                {
                    return GetMessageBody(textPart); // Đệ quy
                }

                // Nếu vẫn lồng nhau tiếp (multipart/mixed), duyệt tiếp con của nó
                foreach (var part in payload.Parts)
                {
                    if (part.Parts != null || part.Body?.Data != null)
                    {
                        return GetMessageBody(part);
                    }
                }
            }

            return body;
        }

        // Hàm giải mã Base64Url của Google sang UTF8 String
        private string DecodeBase64Url(string base64Url)
        {
            try
            {
                string padded = base64Url.Replace("-", "+").Replace("_", "/");
                switch (padded.Length % 4)
                {
                    case 2: padded += "=="; break;
                    case 3: padded += "="; break;
                }
                byte[] data = Convert.FromBase64String(padded);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return "Error decoding content.";
            }
        }

        public async Task<bool> TrashEmailAsync(string messageId)
        {
            try
            {
                await _gmailService.Users.Messages.Trash("me", messageId).ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa email: {ex.Message}");
                return false;
            }
        }
    }
}
