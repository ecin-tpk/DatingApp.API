using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Account;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DatingApp.API.Services
{
    #region Interface
    public interface IFacebookService
    {
        Task<FacebookLoginResponse> GetUser(FacebookLoginRequest model);
    }
    #endregion

    public class FacebookService : IFacebookService
    {
        private readonly HttpClient _httpClient;
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public FacebookService(DataContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/v8.0/")
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Get user
        public async Task<FacebookLoginResponse> GetUser(FacebookLoginRequest model)
        {
            var result = await GetAsync<dynamic>(model.FacebookToken, model.FacebookUID, "fields=name,email,birthday,location,gender,picture.width(500)");
            if (result == null)
            {
                throw new AppException("Invalid Facebook credentials");
            }

            if (await _context.Users.AnyAsync(u => u.FacebookUID == model.FacebookUID))
            {
                return new FacebookLoginResponse
                {
                    Existing = true
                };
            }

            var facebookUser = new FacebookLoginResponse()
            {
                Email = result.email ?? null,
                Name = result.name,
                Gender = result.gender ?? null,
                DateOfBirth = result.birthday ?? null,
                Location = result.location != null ? result.location.name : null,
                FacebookUID = model.FacebookUID,
                Picture = result.picture.data.url,
                Existing = false
            };

            return facebookUser;
        }

        // Helpers
        private async Task<T> GetAsync<T>(string accessToken, string facebookUserId, string args = null)
        {
            var response = await _httpClient.GetAsync($"{facebookUserId}?{args}&access_token={accessToken}");
            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
