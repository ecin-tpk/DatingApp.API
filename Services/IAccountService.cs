using System.Threading.Tasks;
using DatingApp.API.Models.Account;
using DatingApp.API.Models.Users;
using DeviceDetectorNET;

namespace DatingApp.API.Services
{
    public interface IAccountService
    {
        Task Register(RegisterRequest model, string origin);

        void VerifyEmail(string token);

        Task<LoginResponse> Login(LoginRequest model, string ipAddress, DeviceDetector deviceDetector);

        Task ForgotPassword(ForgotPasswordRequest model, string origin);

        Task ResetPassword(ResetPasswordRequest model);

        Task<UserResponse> UpdatePassword(int id, UpdatePasswordRequest model);

        Task<LoginResponse> RefreshToken(string token, string ipAddress, DeviceDetector deviceDetector);

        Task ValidateResetToken(ValidateResetTokenRequest model);

        Task RevokeToken(string token, string ipAddress);
    }
}
