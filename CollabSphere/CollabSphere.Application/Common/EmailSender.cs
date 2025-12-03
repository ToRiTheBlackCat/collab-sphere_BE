using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Common
{
    public class EmailSender
    {
        private IConfiguration _configure;
        public EmailSender(IConfiguration configure)
        {
            _configure = configure;
        }

        /// <summary>
        /// Function using for sending OTP code to email support for signup
        /// </summary>
        /// <param name="toEmail">Receiver email</param>
        /// <returns> OTP code </returns>
        public string SendOTPToEmail(string toEmail)
        {
            var email = _configure["SMTPSettings:Email"] ?? "";
            var password = _configure["SMTPSettings:AppPassword"] ?? "";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true,
            };
            var otpCode = GenerateSecureRandomString(6);
            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = "🔐 COLLAB-SPHERE | SIGNUP OTP CODE 🔐",
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            string htmlBody = $@"
<html>
  <head>
    <style>
      body {{
        background: #f7f9fc;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        margin: 0;
        padding: 0;
      }}
      .container {{
        width: 100%;
        padding: 40px 0;
        display: flex;
        justify-content: center;
      }}
      .card {{
        width: 600px;
        background: linear-gradient(135deg, #2b5876, #4e4376);
        border-radius: 12px;
        padding: 40px;
        color: white !important;
        text-align: center;
        box-shadow: 0 8px 20px rgba(0,0,0,0.1);
      }}
      .otp-box {{
        font-size: 28px;
        font-weight: bold;
        background-color: #ffffff;
        color: #2b5876;
        display: inline-block;
        padding: 12px 24px;
        border-radius: 8px;
        margin: 20px 0;
        letter-spacing: 4px;
        min-width: 180px;
      }}
      h2, p {{
        color: white !important;
        margin: 0 0 20px 0;
      }}
      .footer {{
        font-size: 12px;
        color: #cccccc !important;
        margin-top: 30px;
      }}
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='card'>
        <h2>📧 Verify Your Email</h2>
        <p>Please use the OTP code below to complete your signup:</p>
        <div class='otp-box'>
          {otpCode}
        </div>
        <p>This code will expire in 15 minutes. If you did not request this, please ignore this email.</p>
        <div class='footer'>
          &copy; © 2025 COLLABSPHERE. All rights reserved.
        </div>
      </div>
    </div>
  </body>
</html>";


            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            //For logo
            // Dynamic path to the image in wwwroot/images/logo/logo.jpg
            //string imagePath = Path.Combine("wwwroot", "images", "logo", "logo.jpg");



            //LinkedResource inlineImage = new LinkedResource(imagePath, MediaTypeNames.Image.Png)
            //{
            //    ContentId = "LockImage",
            //    ContentType = new ContentType(MediaTypeNames.Image.Png),
            //    TransferEncoding = TransferEncoding.Base64,
            //    ContentLink = new Uri("cid:LockImage")
            //};
            //avHtml.LinkedResources.Add(inlineImage);

            mailMessage.AlternateViews.Add(avHtml);
            smtpClient.Send(mailMessage);

            return otpCode;
        }

        /// <summary>
        /// Support function using for generate random string with input length
        /// </summary>
        /// <param name="length">Input value of the length of strng</param>
        /// <returns> n lenthg random string</returns>
        private string GenerateSecureRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using var rng = RandomNumberGenerator.Create();
            var result = new char[length];
            var buffer = new byte[sizeof(uint)];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                result[i] = chars[(int)(num % (uint)chars.Length)];
            }

            return new string(result);
        }

        public void SendScheduleMeetingMails(List<string> toEmails, Domain.Entities.Meeting meeting, string meetingUrl, string senderName)
        {
            var email = _configure["SMTPSettings:Email"] ?? "";
            var password = _configure["SMTPSettings:AppPassword"] ?? "";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = "📅 COLLAB-SPHERE | Team Meeting Notification",
                IsBodyHtml = true
            };

            foreach (var mail in toEmails)
            {
                if (!string.IsNullOrWhiteSpace(mail))
                    mailMessage.To.Add(mail);
            }

            string liveBadge = meeting.Status == 1
                ? "<span style='background:#28a745; color:#fff; padding:4px 10px; border-radius:5px; font-size:12px;'>🟢 LIVE</span>"
                : "<span style='background:#f0ad4e; color:#fff; padding:4px 10px; border-radius:5px; font-size:12px;'>⏳ UPCOMING</span>";

            string htmlBody = $@"
<html>
  <head>
    <style>
      body {{
        background-color: #f4f7fa;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        margin: 0;
        padding: 0;
      }}
      .container {{
        width: 100%;
        padding: 40px 0;
        display: flex;
        justify-content: center;
      }}
      .card {{
        width: 600px;
        background-color: #ffffff;
        border-radius: 12px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.08);
        overflow: hidden;
      }}
      .header {{
        background: linear-gradient(135deg, #2b5876, #4e4376);
        color: #ffffff;
        text-align: center;
        padding: 30px 20px;
      }}
      .header h2 {{
        margin: 0;
        font-size: 22px;
        font-weight: 600;
      }}
      .content {{
        padding: 30px 40px;
        color: #333;
      }}
      .content p {{
        margin: 8px 0;
        line-height: 1.5;
      }}
      .schedule-box {{
        margin: 25px 0;
        background-color: #f0f4f8;
        border-left: 5px solid #4e4376;
        padding: 15px 20px;
        border-radius: 6px;
      }}
      .schedule-box p {{
        margin: 5px 0;
      }}
      .btn {{
        display: inline-block;
        background-color: #4e4376;
        color: white !important;
        text-decoration: none;
        padding: 12px 24px;
        border-radius: 6px;
        margin-top: 15px;
        font-weight: 500;
      }}
      .btn:hover {{
        background-color: #2b5876;
      }}
      .footer {{
        background-color: #fafafa;
        color: #888;
        text-align: center;
        font-size: 12px;
        padding: 15px;
      }}
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='card'>
        <div class='header'>
          <h2>📅 CollabSphere Team Meeting Scheduled</h2>
          {liveBadge}
        </div>
        <div class='content'>
          <p>Hello Team Member,</p>
          <p>A new meeting has been scheduled. Please check the details below:</p>

          <div class='schedule-box'>
            <p><strong>🧭 Title:</strong> {meeting.Title}</p>
            <p><strong>📆 Date & Time:</strong> {meeting.ScheduleTime}</p>
            <p><strong>👤 Created By:</strong> {senderName}</p>
            <p><strong>📝 Description:</strong> {meeting.Description}</p>
          </div>

          <a href='{meetingUrl}' class='btn'>🔗 Join meeting now</a>

          <p style='margin-top: 30px;'>
            Please make sure to join on time. You can view the full agenda in your meeting dashboard.
          </p>
        </div>
        <div class='footer'>
          © 2025 COLLABSPHERE. All rights reserved.
        </div>
      </div>
    </div>
  </body>
</html>";

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

            // --- Logo Section ---
            string imagePath = Path.Combine("wwwroot", "images", "logo", "logo.jpg");
            if (File.Exists(imagePath))
            {
                LinkedResource inlineLogo = new LinkedResource(imagePath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "LogoImage",
                    TransferEncoding = TransferEncoding.Base64
                };
                avHtml.LinkedResources.Add(inlineLogo);
            }

            mailMessage.AlternateViews.Add(avHtml);
            smtpClient.Send(mailMessage);
        }

        public void SendSystemReport(string fromEmail, string title, string? contents, List<IFormFile>? Attachments)
        {
            var email = _configure["SMTPSettings:Email"] ?? "";
            var password = _configure["SMTPSettings:AppPassword"] ?? "";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true,
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = @$"⚠️⚠️⚠️ COLLAB-SPHERE | SYSTEM REPORTS ⚠️⚠️⚠️",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            var attachmentListHtml = ""; // Chuỗi HTML để liệt kê tên file trong body mail

            if (Attachments != null && Attachments.Count > 0)
            {
                attachmentListHtml = "<div style='margin-top: 15px; padding: 10px; background: #fff; border: 1px solid #ddd; border-radius: 4px;'><strong>📎 ATTACHED FILES:</strong><ul style='margin: 5px 0 0 20px; padding: 0;'>";

                foreach (var file in Attachments)
                {
                    if (file.Length > 0)
                    {
                        // BƯỚC 1: Copy file vào MemoryStream (RAM)
                        var ms = new MemoryStream();
                        file.CopyTo(ms);

                        // BƯỚC 2: QUAN TRỌNG - Reset vị trí đọc về đầu (0)
                        ms.Position = 0;

                        // BƯỚC 3: Tạo Attachment từ MemoryStream
                        // Lưu ý: Không dùng 'using' cho MemoryStream ở đây vì Attachment cần stream tồn tại đến khi hàm Send() chạy xong
                        var attachment = new Attachment(ms, file.FileName, file.ContentType);
                        mailMessage.Attachments.Add(attachment);

                        // Thêm tên file vào danh sách hiển thị HTML
                        attachmentListHtml += $"<li style='color: #555; font-size: 13px;'>{file.FileName} ({file.Length / 1024} KB)</li>";
                    }
                }
                attachmentListHtml += "</ul></div>";
            }
            else
            {
                attachmentListHtml = "<div style='margin-top: 10px; font-size: 12px; color: #888;'><em>No files attached.</em></div>";
            }

            string htmlBody = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            .alert-container {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 650px; margin: 0 auto; border: 2px solid #dc2626; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
            .alert-header {{ background-color: #991b1b; padding: 25px; color: white; text-align: center; border-bottom: 4px solid #ef4444; }}
            .alert-body {{ padding: 30px; background-color: #fff1f2; }}
            .label {{ color: #7f1d1d; font-size: 11px; font-weight: bold; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 5px; display: block; }}
            .value {{ font-size: 15px; color: #111; margin-bottom: 20px; font-weight: 500; }}
            .content-box {{ background-color: #ffffff; padding: 20px; border-left: 5px solid #dc2626; border-radius: 4px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
            .footer {{ background-color: #fee2e2; padding: 15px; text-align: center; font-size: 12px; color: #b91c1c; border-top: 1px solid #fecaca; }}
            .btn-action {{ display: inline-block; padding: 10px 20px; background-color: #dc2626; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 10px; }}
        </style>
    </head>
    <body>
        <div class='alert-container'>
            <div class='alert-header'>
                <h1 style='margin: 0; font-size: 28px; letter-spacing: 2px;'>⚠️ SYSTEM ALERT ⚠️</h1>
            </div>
            
            <div class='alert-body'>
                <span class='label'>REPORTER / NGƯỜI BÁO CÁO</span>
                <div class='value' style='display: flex; align-items: center;'>
                    <span style='background: #fee2e2; color: #991b1b; padding: 4px 8px; border-radius: 4px; font-weight: bold;'>{fromEmail}</span>
                </div>

                <span class='label'>ISSUE SUMMARY / TIÊU ĐỀ LỖI</span>
                <div class='value' style='font-size: 18px; color: #dc2626; font-weight: bold;'>
                    {title}
                </div>

                <hr style='border: 0; border-top: 1px dashed #fca5a5; margin: 20px 0;' />

                <span class='label'>DETAILED DESCRIPTION / CHI TIẾT</span>
                <div class='content-box'>
                    {contents ?? "<em>No content provided.</em>"}
                </div>
                
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='mailto:{fromEmail}' class='btn-action'>REPLY TO USER</a>
                </div>
            </div>

            <div class='footer'>
                <strong>⚡ ACTION REQUIRED</strong><br/>
                This is a critical system notification. Please investigate immediately.<br/>
                Attachments included: {(Attachments != null ? Attachments.Count : 0)} files.
            </div>
        </div>
    </body>
    </html>";
            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            mailMessage.AlternateViews.Add(avHtml);
            smtpClient.Send(mailMessage);

        }

        public async Task SendNotiEmailsForCheckpoint(HashSet<string> receivers, Checkpoint? checkpoint)
        {
            var email = _configure["SMTPSettings:Email"] ?? "";
            var password = _configure["SMTPSettings:AppPassword"] ?? "";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = @$"🎯 COLLAB-SPHERE | Checkpoint Assigned: {checkpoint.Title}",
                IsBodyHtml = true
            };

            foreach (var rec in receivers)
            {
                if (!string.IsNullOrWhiteSpace(rec))
                    mailMessage.To.Add(rec);
            }
            var checkpointUrl = "https://collabsphere.space";

            // Safe null handling
            string desc = string.IsNullOrEmpty(checkpoint.Description) ? "No description provided." : checkpoint.Description;
            string dateRange = $"{checkpoint.StartDate:MMM dd, yyyy} - {checkpoint.DueDate:MMM dd, yyyy}";

            string htmlBody = $@"
<html>
  <head>
    <style>
      body {{
        background-color: #f4f7fa;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        margin: 0;
        padding: 0;
      }}
      .container {{
        width: 100%;
        padding: 40px 0;
        display: flex;
        justify-content: center;
      }}
      .card {{
        width: 600px;
        background-color: #ffffff;
        border-radius: 12px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.08);
        overflow: hidden;
      }}
      .header {{
        background: linear-gradient(135deg, #2b5876, #4e4376);
        color: #ffffff;
        text-align: center;
        padding: 30px 20px;
      }}
      .header h2 {{
        margin: 0;
        font-size: 22px;
        font-weight: 600;
      }}
      .content {{
        padding: 30px 40px;
        color: #333;
      }}
      .content p {{
        margin: 8px 0;
        line-height: 1.5;
      }}
      /* Matching the Meeting style box exactly */
      .schedule-box {{
        margin: 25px 0;
        background-color: #f0f4f8;
        border-left: 5px solid #4e4376;
        padding: 15px 20px;
        border-radius: 6px;
      }}
      .schedule-box p {{
        margin: 5px 0;
      }}
      .btn {{
        display: inline-block;
        background-color: #4e4376;
        color: white !important;
        text-decoration: none;
        padding: 12px 24px;
        border-radius: 6px;
        margin-top: 15px;
        font-weight: 500;
      }}
      .btn:hover {{
        background-color: #2b5876;
      }}
      .footer {{
        background-color: #fafafa;
        color: #888;
        text-align: center;
        font-size: 12px;
        padding: 15px;
      }}
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='card'>
        <div class='header'>
          <h2>🎯 Checkpoint Assigned</h2>
        </div>
        <div class='content'>
          <p>Hello Team Member,</p>
          <p>a new checkpoint had been assigned to you. Please check the details below:</p>

          <div class='schedule-box'>
            <p><strong>📌 Checkpoint:</strong> {checkpoint.Title}</p>
            <p><strong>⏳ Timeline:</strong> {dateRange}</p>
            <p><strong>📝 Description:</strong> {desc}</p>
          </div>

          <div style='text-align: center;'>
             <a href='{checkpointUrl}' class='btn'>👉 View Checkpoint Details</a>
          </div>

          <p style='margin-top: 30px; font-size: 13px; color: #777; text-align: center;'>
            Please ensure all deliverables are uploaded before the deadline.
          </p>
        </div>
        <div class='footer'>
          © 2025 COLLABSPHERE. All rights reserved.
        </div>
      </div>
    </div>
  </body>
</html>";

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

            string imagePath = Path.Combine("wwwroot", "images", "logo", "logo.jpg");
            if (File.Exists(imagePath))
            {
                LinkedResource inlineLogo = new LinkedResource(imagePath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "LogoImage",
                    TransferEncoding = TransferEncoding.Base64
                };
                avHtml.LinkedResources.Add(inlineLogo);
            }

            mailMessage.AlternateViews.Add(avHtml);

            if (mailMessage.To.Count > 0)
            {
                smtpClient.Send(mailMessage);
            }
        }

        public async Task SendNotiEmailsForTeamEva(HashSet<string> receivers, TeamEvaluation teamEva, List<(string gradeComponentName, decimal percentage, decimal score, string comment)> sendMailDetailList)
        {
            var email = _configure["SMTPSettings:Email"] ?? "";
            var password = _configure["SMTPSettings:AppPassword"] ?? "";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = "🏆 COLLAB-SPHERE | Team Evaluation Results",
                IsBodyHtml = true
            };

            foreach (var rec in receivers)
            {
                if (!string.IsNullOrWhiteSpace(rec))
                    mailMessage.To.Add(rec);
            }

            // --- 1. Generate the Detailed List HTML ---
            string detailsHtml = "";
            foreach (var detail in sendMailDetailList)
            {
                // detail.Item1 = Name, Item2 = %, Item3 = Score, Item4 = Comment
                detailsHtml += $@"
                <div style='border-bottom: 1px solid #eee; padding: 12px 0;'>
                    <div style='display:flex; justify-content:space-between; align-items:center;'>
                        <span style='font-weight:bold; color:#2b5876; font-size:15px;'>{detail.gradeComponentName} <span style='font-size:12px; color:#888; font-weight:normal;'>({detail.percentage}%)</span></span>
                        <span style='font-weight:bold; color:#333; background:#f0f4f8; padding:2px 8px; border-radius:4px;'>{detail.score} / 10</span>
                    </div>
                    <div style='margin-top:6px; font-size:13px; color:#555; font-style:italic;'>
                        {(string.IsNullOrWhiteSpace(detail.comment) ? "No specific comment." : detail.comment)}
                    </div>
                </div>";
            }

            // --- 2. Build the Main HTML ---
            string finalGradeDisplay = teamEva.FinalGrade.HasValue ? $"{teamEva.FinalGrade.Value:0.##} / 10" : "N/A";
            string generalComment = string.IsNullOrWhiteSpace(teamEva.Comment) ? "No general comment provided." : teamEva.Comment;

            string htmlBody = $@"
<html>
  <head>
    <style>
      body {{
        background-color: #f4f7fa;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        margin: 0;
        padding: 0;
      }}
      .container {{
        width: 100%;
        padding: 40px 0;
        display: flex;
        justify-content: center;
      }}
      .card {{
        width: 600px;
        background-color: #ffffff;
        border-radius: 12px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.08);
        overflow: hidden;
      }}
      .header {{
        background: linear-gradient(135deg, #2b5876, #4e4376);
        color: #ffffff;
        text-align: center;
        padding: 30px 20px;
      }}
      .header h2 {{
        margin: 0;
        font-size: 22px;
        font-weight: 600;
      }}
      .content {{
        padding: 30px 40px;
        color: #333;
      }}
      /* The Meeting/Checkpoint Style Box */
      .schedule-box {{
        margin: 25px 0;
        background-color: #f0f4f8;
        border-left: 5px solid #4e4376;
        padding: 20px;
        border-radius: 6px;
      }}
      .schedule-box p {{
        margin: 8px 0;
      }}
      .details-section {{
        margin-top: 30px;
      }}
      .details-header {{
        font-size: 14px;
        font-weight: bold;
        color: #4e4376;
        text-transform: uppercase;
        letter-spacing: 1px;
        border-bottom: 2px solid #f0f4f8;
        padding-bottom: 10px;
        margin-bottom: 10px;
      }}
      .btn {{
        display: inline-block;
        background-color: #4e4376;
        color: white !important;
        text-decoration: none;
        padding: 12px 24px;
        border-radius: 6px;
        margin-top: 25px;
        font-weight: 500;
      }}
      .btn:hover {{
        background-color: #2b5876;
      }}
      .footer {{
        background-color: #fafafa;
        color: #888;
        text-align: center;
        font-size: 12px;
        padding: 15px;
      }}
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='card'>
        <div class='header'>
          <h2>🏆 Team Evaluation Results</h2>
        </div>
        <div class='content'>
          <p>Hello Team,</p>
          <p>Your lecturer has submitted an evaluation for your team's performance. Here is the summary:</p>

          <div class='schedule-box'>
            <p style='font-size: 18px; color: #4e4376;'><strong>⭐ Final Grade: {finalGradeDisplay}</strong></p>
            <hr style='border: 0; border-top: 1px solid #dde2e9; margin: 10px 0;'/>
            <p><strong>💬 Lecturer Comment:</strong></p>
            <p style='font-style: italic; color: #555;'>""{generalComment}""</p>
          </div>

          <div class='details-section'>
            <div class='details-header'>📝 Grade Breakdown</div>
            {detailsHtml}
          </div>

          <div style='text-align: center;'>
             <a href='https://collabsphere.space' class='btn'>View Full Report</a>
          </div>
        </div>
        <div class='footer'>
          © 2025 COLLABSPHERE. All rights reserved.
        </div>
      </div>
    </div>
  </body>
</html>";

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

            // --- Logo Section ---
            string imagePath = Path.Combine("wwwroot", "images", "logo", "logo.jpg");
            if (File.Exists(imagePath))
            {
                LinkedResource inlineLogo = new LinkedResource(imagePath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "LogoImage",
                    TransferEncoding = TransferEncoding.Base64
                };
                avHtml.LinkedResources.Add(inlineLogo);
            }

            mailMessage.AlternateViews.Add(avHtml);

            if (mailMessage.To.Count > 0)
            {
                smtpClient.Send(mailMessage);
            }
        }

        public async Task SendNotiEmailsForMileEva(HashSet<string> receivers, string teamName, string milestoneTitle, MilestoneEvaluation? milestoneEvaluation)
        {
            var email = _configure["SMTPSettings:Email"] ?? "";
            var password = _configure["SMTPSettings:AppPassword"] ?? "";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = @$"🏆 COLLAB-SPHERE | Milestone Evaluated: {milestoneTitle}",
                IsBodyHtml = true
            };

            foreach (var rec in receivers)
            {
                if (!string.IsNullOrWhiteSpace(rec))
                    mailMessage.To.Add(rec);
            }

            // Safe Data Handling
            string scoreDisplay = milestoneEvaluation?.Score.HasValue == true
                ? $"{milestoneEvaluation.Score.Value:0.##} / 10"
                : "N/A";

            string commentDisplay = string.IsNullOrWhiteSpace(milestoneEvaluation?.Comment)
                ? "No specific comment provided."
                : milestoneEvaluation.Comment;

            // Standard Link
            var dashboardUrl = "https://collabsphere.space";

            string htmlBody = $@"
<html>
  <head>
    <style>
      body {{
        background-color: #f4f7fa;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        margin: 0;
        padding: 0;
      }}
      .container {{
        width: 100%;
        padding: 40px 0;
        display: flex;
        justify-content: center;
      }}
      .card {{
        width: 600px;
        background-color: #ffffff;
        border-radius: 12px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.08);
        overflow: hidden;
      }}
      .header {{
        /* Standard Blue/Purple Gradient */
        background: linear-gradient(135deg, #2b5876, #4e4376);
        color: #ffffff;
        text-align: center;
        padding: 30px 20px;
      }}
      .header h2 {{
        margin: 0;
        font-size: 22px;
        font-weight: 600;
      }}
      .content {{
        padding: 30px 40px;
        color: #333;
      }}
      .content p {{
        margin: 8px 0;
        line-height: 1.5;
      }}
      /* The requested Meeting Style Box */
      .schedule-box {{
        margin: 25px 0;
        background-color: #f0f4f8;
        border-left: 5px solid #4e4376;
        padding: 20px;
        border-radius: 6px;
      }}
      .schedule-box p {{
        margin: 8px 0;
        font-size: 15px;
      }}
      .btn {{
        display: inline-block;
        background-color: #4e4376;
        color: white !important;
        text-decoration: none;
        padding: 12px 24px;
        border-radius: 6px;
        margin-top: 15px;
        font-weight: 500;
      }}
      .btn:hover {{
        background-color: #2b5876;
      }}
      .footer {{
        background-color: #fafafa;
        color: #888;
        text-align: center;
        font-size: 12px;
        padding: 15px;
      }}
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='card'>
        <div class='header'>
          <h2>🏆 Milestone Results Available</h2>
        </div>
        <div class='content'>
          <p>Hello Team,</p>
          <p>Your lecturer has graded the milestone <strong>{milestoneTitle}</strong> for team <strong>{teamName}</strong>. Please see the evaluation details below:</p>

          <div class='schedule-box'>
            <p><strong>👥 Team:</strong> {teamName}</p>
            <p><strong>🏁 Milestone:</strong> {milestoneTitle}</p>
            <p><strong>⭐ Score:</strong> <span style='font-weight:bold; color:#4e4376;'>{scoreDisplay}</span></p>
            <p><strong>💬 Comment:</strong></p>
            <p style='font-style: italic; color: #555; margin-left: 10px;'>""{commentDisplay}""</p>
          </div>

          <div style='text-align: center;'>
             <a href='{dashboardUrl}' class='btn'>👉 View Full Report</a>
          </div>

          <p style='margin-top: 30px; font-size: 13px; color: #777; text-align: center;'>
            Keep up the good work!
          </p>
        </div>
        <div class='footer'>
          © 2025 COLLABSPHERE. All rights reserved.
        </div>
      </div>
    </div>
  </body>
</html>";

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

            // --- Logo Section ---
            string imagePath = Path.Combine("wwwroot", "images", "logo", "logo.jpg");
            if (File.Exists(imagePath))
            {
                LinkedResource inlineLogo = new LinkedResource(imagePath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "LogoImage",
                    TransferEncoding = TransferEncoding.Base64
                };
                avHtml.LinkedResources.Add(inlineLogo);
            }

            mailMessage.AlternateViews.Add(avHtml);

            if (mailMessage.To.Count > 0)
            {
                smtpClient.Send(mailMessage);
            }
        }
    }
}
