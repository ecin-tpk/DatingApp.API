using System;
using System.Threading.Tasks;
using DatingApp.API.Models.Account;

namespace DatingApp.API.Services
{
    public interface IFacebookService
    {
        Task<FacebookLoginResponse> GetUser(string facebookToken);
    }
}
