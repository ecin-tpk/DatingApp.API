using DatingApp.API.Entities;

namespace DatingApp.API.Services
{
    public interface IEmailService
    {
        void Send(string recipientEmail, string subject, string htmlBody, string senderEmail = null);

        void SendVerificationEmail(User user, string origin);

        void SendAlreadyRegisteredEmail(string email, string origin);

        void SendForgotPasswordEmail(User user, string origin);
    }
}
