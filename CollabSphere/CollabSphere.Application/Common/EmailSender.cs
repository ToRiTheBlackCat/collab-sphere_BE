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
          &copy; 2025 COLLABSPHERE. All rights reserved.
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
    }
}
