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
        Task<FacebookLoginResponse> GetUser(FacebookLoginRequest model, string origin);
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

        public async Task<FacebookLoginResponse> GetUser(FacebookLoginRequest model, string origin)
        {
            var result = await GetAsync<dynamic>(model.facebookToken, model.facebookUserId, "fields=name,email,birthday,location,gender,picture.width(500)");
            if (result == null)
            {
                throw new AppException("Invalid credentials");
            }

            var email = (string)result.email;

            var existingUserWithSameEmail = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

            var account = new FacebookLoginResponse()
            {
                Email = existingUserWithSameEmail == null ? result.email : null,
                Name = result.name,
                Gender = result.gender,
                DateOfBirth = result.birthday,
                City = result.location.name,
                FacebookUID = model.facebookUserId,
                Picture = result.picture.data.url
            };

            return account;
        }

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
