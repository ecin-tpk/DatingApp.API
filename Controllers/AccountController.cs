using System.Threading.Tasks;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Models.Account;
using DatingApp.API.Services;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // POST: Register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            await _accountService.Register(model, Request.Headers["origin"]);
            return Ok(new { message = "Registration successfull, please check your email for verification instructions" });
        }

        // POST: Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            model.Role = Role.User;

            var dd = new DeviceDetector(Request.Headers["User-Agent"]);

            var response = await _accountService.Login(model, IpAddress(), dd);

            Response.SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        // POST: Login with Facebook
        [HttpPost("facebook-login")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest model)
        {
            var dd = new DeviceDetector(Request.Headers["User-Agent"]);

            var response = await _accountService.FacebookLogin(model, IpAddress(), dd);

            Response.SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        // POST: Login with Google
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest model)
        {
            var dd = new DeviceDetector(Request.Headers["User-Agent"]);

            var response = await _accountService.GoogleLogin(model, IpAddress(), dd, Request.Headers["origin"]);

            Response.SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        // POST: Verify email
        [HttpGet("verify-email")]
        public IActionResult VerifyEmail([FromQuery] string token)
        {
            _accountService.VerifyEmail(token);

            return Ok(new { message = "Verification successful, you can now login" });
        }

        // POST: Forgot password (when user forgot their password they send email to /account/forgot-password then we send an email that contains a token to reset their password)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            await _accountService.ForgotPassword(model, Request.Headers["orgin"]);

            return Ok(new { message = "Check your email for instructions" });
        }

        // POST: Reset password 
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            await _accountService.ResetPassword(model);

            return Ok(new { message = "Password reset successful, you can now login" });
        }

        // PUT: Update password
        [Authorize]
        [HttpPut("{id:int}/update-password")]
        public async Task<IActionResult> UpdatePassword(int id, UpdatePasswordRequest model)
        {
            // Users can update their own password and admins can update any user's password
            if (id != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _accountService.UpdatePassword(id, model);

            return Ok("Password updated successfully");
        }

        // POST: Use refresh token to get a new jwt token
        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponse>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var dd = new DeviceDetector(Request.Headers["User-Agent"]);

            var response = await _accountService.RefreshToken(refreshToken, IpAddress(), dd);

            Response.SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        // POST: Revoke a reset token so it can no longer be used to generate jwt token
        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken(RevokeTokenRequest model)
        {
            // If user didn't send which token to revoke, the current token in cookie would be revoked
            var token = model.Token ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token.Trim()))
            {
                return BadRequest(new { message = "Invalid token" });
            }

            // Admin can revoke any token while users can only revoke their ones
            if (!User.OwnsToken(token) && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _accountService.RevokeToken(token, IpAddress());

            return Ok(new { message = "Token revoked" });
        }

        // POST: Resend verification email
        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail(ForgotPasswordRequest model)
        {
            await _accountService.ResendVerificationEmail(model);

            return Ok(new { message = "Email sent, please check your email for verification instructions" });
        }

        // POST: Validate reset token (to verify that password reset link is still valid)
        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken(TokenRequest model)
        {
            await _accountService.ValidateResetToken(model);

            return Ok(new { message = "Token is valid" });
        }

        // POST: Add FCM token
        [Authorize]
        [HttpPut("fcm-token")]
        public async Task<IActionResult> AddFcmToken(TokenRequest model)
        {
            await _accountService.AddFcmToken(User.Id, model);
            return Ok();
        }

        // GET: Get list of FCM tokens
        [Authorize]
        [HttpGet("{id:int}/fcm-tokens")]
        public async Task<IActionResult> GetFcmTokens(int id)
        {
            //if (id != User.Id && User.Role != Role.Admin)
            //{
            //    return Unauthorized(new { message = "Unauthorized" });
            //}
            var tokens = await _accountService.GetFcmTokens(id);
            return Ok(tokens);
        }

        // Helpers
        // Get IP address
        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwared-For"))
            {
                return Request.Headers["X-Forwarded-For"];
            }
            else
            {
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}
