using System;
using System.Threading.Tasks;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Account;
using DatingApp.API.Models.Users;
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

        // POST: Login with facebook
        [HttpPost("login/facebook")]
        public async Task<IActionResult> FacebookLoginAsync([FromBody] FacebookLoginRequest model)
        {
            var dd = new DeviceDetector(Request.Headers["User-Agent"]);

            var response = await _accountService.FacebookLogin(model, IpAddress(), dd);

            Response.SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        // POST: Verify email
        [HttpPost("verify-email")]
        public IActionResult VerifyEmail(VerifyEmailRequest model)
        {
            _accountService.VerifyEmail(model.Token);

            return Ok(new { message = "Verification successful, you can now login" });
        }

        // POST: Forgot password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequest model)
        {
            await _accountService.ForgotPassword(model, Request.Headers["orgin"]);

            return Ok(new { message = "Please check your email for password reset instructions" });
        }

        // POST: Reset password (when user forgot their password they send email to /account/forgot-password then we send an email that contains a token to verify
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest model)
        {
            await _accountService.ResetPassword(model);

            return Ok(new { message = "Password reset successful, you can now login" });
        }

        // PUT: Update password
        [HttpPut("{id:int}/update-password")]
        public async Task<IActionResult> UpdatePassword(int id, UpdatePasswordRequest model)
        {
            // Users can update their own password and admins can update any user's password
            if (id != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var user = await _accountService.UpdatePassword(id, model);

            return Ok(user);
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
            var token = model.Token ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            if (!User.OwnsToken(token) && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _accountService.RevokeToken(token, IpAddress());

            return Ok(new { message = "Token revoked" });
        }

        // Validate reset token
        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken(ValidateResetTokenRequest model)
        {
            await _accountService.ValidateResetToken(model);

            return Ok(new { message = "Token is valid" });
        }

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
