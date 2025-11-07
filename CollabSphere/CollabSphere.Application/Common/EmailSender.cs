using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;

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

    }
}
