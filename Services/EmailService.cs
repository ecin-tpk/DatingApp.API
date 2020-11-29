using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace DatingApp.API.Services
{
    #region Interface
    public interface IEmailService
    {
        void Send(string recipientEmail, string subject, string htmlBody, string senderEmail = null);
        void SendVerificationEmail(User user, string origin);
        void SendAlreadyRegisteredEmail(string email, string origin);
        void SendForgotPasswordEmail(User user, string origin);
    }
    #endregion

    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;

        public EmailService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public void Send(string recipientEmail, string subject, string htmlBody, string senderEmail = null)
        {
            // Create message
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(senderEmail ?? _appSettings.EmailFrom);
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            // Send email
            using (var smtp = new SmtpClient())
            {
                smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(_appSettings.SmtpUsername, _appSettings.SmtpPassword);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }

        // Send verification email
        public void SendVerificationEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var verifyUrl = $"{origin}/api/account/verify-email?token={user.VerificationToken}";
                message = $@"<p>Please click the link below to verify your email address:</p><p><a href=""{verifyUrl}"">{verifyUrl}</a></p><p><code>{user.VerificationToken}</code></p>";
            }
            else
            {
                var verifyUrl = $"http://localhost:5000/api/account/verify-email?token={user.VerificationToken}";
                message = $@"<p>Please click the link below to verify your email address:</p><p><a href=""{verifyUrl}"">{verifyUrl}</a></p><p><code>{user.VerificationToken}</code></p>";
                //message = $@"<p>Please use the below token to verify your email address with the <code>/api/account/verify-email</code> api route:</p><p><code>{user.VerificationToken}</code></p>";
            }

            Send(recipientEmail: user.Email, subject: "Complete your Dating App registration!", htmlBody: $@"<h4>Verify Email</h4><p>Thanks for registering!</p>{message}");
        }

        // Send already registerd email
        public void SendAlreadyRegisteredEmail(string email, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                message = $@"<p>If you don't know your password please vist <a href=""{origin}/accounts/forgot-password"">Forgot Password</a></p>";
            }
            else
            {
                message = $@"<p>If you don't know your password you can reset it via the <code>/account/forgot-password</code> api route</p>";
            }

            Send(recipientEmail: email, subject: "Dating App registration - Email already registered.", htmlBody: $@"<h4>Email already Registered</h4><p>Your email <strong>{email}</strong> is already registered.</p>{message}");
        }

        // Send forgot password email that contains a token
        public void SendForgotPasswordEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetPasswordUrl = $"{origin}/account/reset-password?token={user.ResetToken}";
                message = $@"<p>Please click the link below to reset your password, the link will be valid for 1 day:</p><p><a href=""{resetPasswordUrl}"">{resetPasswordUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the token below to reset your password with the <code>/account/reset-password</code> api route:</p><p><code>{user.ResetToken}</code></p>";
            }

            Send(recipientEmail: user.Email, subject: "Reset your password at DatingApp", htmlBody: $@"<h4>Reset Password Email</h4>{message}");
        }
    }
}
