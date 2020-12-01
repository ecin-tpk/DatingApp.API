using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        // Invoke jwt token
        public async Task Invoke(HttpContext httpContext, DataContext dataContext)
        {
            var token = httpContext.Request.Headers["Authorization"].FirstOrDefault();

            var test = httpContext.Request.Query["access_token"];

            if (token != null && token.StartsWith("Bearer") && token?.Split(" ").Last() != null)
            {
                await AttachUserToContext(httpContext, dataContext, token?.Split(" ").Last());
            }

            if (test.ToString() != null)
            {
                await AttachUserToContext(httpContext, dataContext, test);
            }

            await _next(httpContext);
        }

        // Attach user to http context
        private async Task AttachUserToContext(HttpContext httpContext, DataContext dataContext, string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Secret));

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    // Set clock skew to zero so tokens expire exactly at token expiration time
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userId = int.Parse(jwtToken.Claims.First(c => c.Type == "id").Value);

                var user = await dataContext.Users.FindAsync(userId);
                user.LastActive = DateTime.Now;

                dataContext.Users.Update(user);
                await dataContext.SaveChangesAsync();

                httpContext.Items["User"] = user;

                httpContext.User.Identities.FirstOrDefault().AddClaim(
                    new Claim("user_id", user.Id.ToString())
                );
            }
            catch { }
        }
    }
}
