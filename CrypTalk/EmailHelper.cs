using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace CrypTalk
{
    public static class EmailHelper
    {
        private const string SMTP_HOST = "smtp.gmail.com";
        private const int SMTP_PORT = 587;
        private const string SENDER_EMAIL = "24521466@gm.uit.edu.vn";
        private const string SENDER_PASSWORD = "ggodihqlnphrwpat";
        private const string SENDER_NAME = "CrypTalk Support";

        public static async Task<bool> SendOTP(string recipientEmail, string username, string otpCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(SENDER_NAME, SENDER_EMAIL));
                message.To.Add(new MailboxAddress(username, recipientEmail));
                message.Subject = "CrypTalk - Password Reset OTP Code";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                                <h2 style='color: #0a1250;'>🔐 Password Reset Request</h2>
                                <p>Hello <strong>{username}</strong>,</p>
                                <p>You have requested to reset your password. Please use the following OTP code:</p>
                                <div style='background: #f0f0f0; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                                    {otpCode}
                                </div>
                                <p style='color: #666;'>This code will expire in <strong>5 minutes</strong>.</p>
                                <p style='color: #999; font-size: 12px;'>If you didn't request this, please ignore this email.</p>
                                <hr style='margin: 20px 0;' />
                                <p style='color: #999; font-size: 11px;'>CrypTalk - Secure Messaging Platform</p>
                            </div>
                        </body>
                        </html>
                    "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(SMTP_HOST, SMTP_PORT, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(SENDER_EMAIL, SENDER_PASSWORD);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Email Error] {ex.Message}");
                return false;
            }
        }

        public static string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}