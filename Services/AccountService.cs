using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Account;
using DatingApp.API.Models.Users;
using DeviceDetectorNET;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        private readonly AppSettings _appSettings;

        private readonly IEmailService _emailService;

        private readonly IUserService _userService;

        public AccountService(DataContext context, IMapper mapper, IOptions<AppSettings> appSettings, IEmailService emailService, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _emailService = emailService;
            _userService = userService;
        }

        // Register
        public async Task Register(RegisterRequest model, string origin)
        {
            // Send already registered error in email
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                _emailService.SendAlreadyRegisteredEmail(model.Email, origin);
                return;
            }

            var userToCreate = _mapper.Map<User>(model);
            userToCreate.Created = DateTime.Now;
            userToCreate.Role = Role.User;
            userToCreate.Status = Status.Active;
            userToCreate.VerificationToken = RandomTokenString();

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            userToCreate.PasswordHash = passwordHash;
            userToCreate.PasswordSalt = passwordSalt;

            _context.Users.Add(userToCreate);
            await _context.SaveChangesAsync();

            _emailService.SendVerificationEmail(userToCreate, origin);
        }

        // Login
        public async Task<LoginResponse> Login(LoginRequest model, string ipAddress, DeviceDetector deviceDetector)
        {
            var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(x => x.Email == model.Email);
            //var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user.Role != Role.Admin && model.Role == Role.Admin)
            {
                throw new AppException("Not eligible");
            }
            if (user == null || !user.IsVerified || !VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new AppException("Email or password is incorrect");
            }

            var refreshToken = GenerateRefreshToken(ipAddress, deviceDetector);

            user.RefreshTokens.Add(refreshToken);

            _context.Update(user);

            await _context.SaveChangesAsync();

            var jwtToken = GenerateJwtToken(user);

            var response = _mapper.Map<LoginResponse>(user);
            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;

            return response;
        }

        // Verify email after register
        public async void VerifyEmail(string token)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                throw new AppException("Verfication failed");
            }
            user.Verified = DateTime.Now;
            user.VerificationToken = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // Forgot password
        public async Task ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) return;

            // Create a reset token that expires after 1 day
            user.ResetToken = RandomTokenString();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            _emailService.SendForgotPasswordEmail(user, origin);
        }

        // Reset password in case user forgot their password
        public async Task ResetPassword(ResetPasswordRequest model)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.ResetToken == model.Token && u.ResetTokenExpires > DateTime.Now);
            if (user == null)
            {
                throw new AppException("Invalid token");
            }

            byte[] passwordHash, passwordSalt;

            CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordReset = DateTime.Now;
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();
        }

        // Update password
        public async Task<UserResponse> UpdatePassword(int id, UpdatePasswordRequest model)
        {
            var userInDb = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (userInDb == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            if (!VerifyPasswordHash(model.CurrentPassword, userInDb.PasswordHash, userInDb.PasswordSalt))
            {
                throw new AppException("Wrong password");
            }

            // Hash password
            byte[] passwordHash, passwordSalt;

            CreatePasswordHash(model.NewPassword, out passwordHash, out passwordSalt);

            userInDb.PasswordHash = passwordHash;
            userInDb.PasswordSalt = passwordSalt;
            userInDb.Updated = DateTime.Now;

            _context.Users.Update(userInDb);

            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<UserResponse>(userInDb);
            }

            throw new AppException("Update password failed");
        }

        // Hash password
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Generate random string for verify token and refersh token
        private string RandomTokenString()
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[40];
                rngCryptoServiceProvider.GetBytes(randomBytes);

                return BitConverter.ToString(randomBytes).Replace("-", "");
            }
        }

        // Generate jwt token
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim("id", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(15),
                SigningCredentials = creds
            };

            var tokenHandlder = new JwtSecurityTokenHandler();

            var token = tokenHandlder.CreateToken(tokenDescriptor);

            return tokenHandlder.WriteToken(token);
        }

        // Verifiy password hash
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Use refresh token to get a new jwt token (auto login)
        public async Task<LoginResponse> RefreshToken(string token, string ipAddress, DeviceDetector dd)
        {
            var newRefreshToken = GenerateRefreshToken(ipAddress, dd);

            var (refreshToken, user) = await GetRefreshToken(token);
            refreshToken.Token = newRefreshToken.Token;
            refreshToken.Expires = newRefreshToken.Expires;
            refreshToken.Created = newRefreshToken.Created;
            refreshToken.CreatedByIp = newRefreshToken.CreatedByIp;
            refreshToken.Device = newRefreshToken.Device;
            refreshToken.Application = newRefreshToken.Application;
            refreshToken.LastActive = newRefreshToken.LastActive;
            //refreshToken.Revoked = DateTime.Now;
            //refreshToken.RevokedByIp = ipAddress;
            //refreshToken.ReplaceByToken = newRefreshToken.Token;

            _context.Update(user);

            await _context.SaveChangesAsync();

            var jwtToken = GenerateJwtToken(user);

            var response = _mapper.Map<LoginResponse>(user);
            response.JwtToken = jwtToken;
            response.RefreshToken = newRefreshToken.Token;

            return response;
        }

        // Revoke token
        public async Task RevokeToken(string token, string ipAddress)
        {
            var (refreshToken, user) = await GetRefreshToken(token);
            //refreshToken.Revoked = DateTime.Now;
            //refreshToken.RevokedByIp = ipAddress;
            user.RefreshTokens.Remove(refreshToken);

            _context.Update(user);

            await _context.SaveChangesAsync();
        }

        // Validate reset token
        public async Task ValidateResetToken(ValidateResetTokenRequest model)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.ResetToken == model.Token && u.ResetTokenExpires > DateTime.Now);

            if (user == null)
            {
                throw new AppException("Invalid token");
            }
        }

        // 
        private async Task<(RefreshToken, User)> GetRefreshToken(string token)
        {
            var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null)
            {
                throw new AppException("Invalid token");
            }

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);
            if (refreshToken.IsExpired)
            {
                user.RefreshTokens.Remove(refreshToken);

                _context.Update(user);

                await _context.SaveChangesAsync();

                throw new AppException("Invalid token");
            }

            return (refreshToken, user);
        }

        // Generate refresh token
        private RefreshToken GenerateRefreshToken(string ipAddress, DeviceDetector dd)
        {
            dd.Parse();

            var refreshToken = new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now,
                CreatedByIp = ipAddress,
                LastActive = DateTime.Now,
            };

            if (!dd.IsBot())
            {
                try
                {
                    var clientInfo = dd.GetClient().ToString().Replace(" ", "").Split("\n")[1].Split(":")[1].Replace(";", "");
                    var osInfo = dd.GetOs().ToString().Replace(" ", "").Split("\n")[1].Split(":")[1].Replace(";", "");
                    var device = dd.GetDeviceName();
                    var brand = dd.GetBrandName();
                    var model = dd.GetModel();

                    refreshToken.Device = model + " " + brand + " " + osInfo + " " + device;
                    refreshToken.Application = clientInfo;
                }
                catch { }
            }

            return refreshToken;
        }


    }
}
