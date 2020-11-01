using System.Threading.Tasks;
using DatingApp.API.Models.Account;

namespace DatingApp.API.Services
{
    public interface IFacebookService
    {
        Task<FacebookLoginResponse> GetUser(FacebookLoginRequest model, string origin);
    }
}
