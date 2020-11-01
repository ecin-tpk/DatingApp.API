using System;
using System.Threading.Tasks;

namespace DatingApp.API.Services
{
    public interface ILikeService
    {
        Task<bool> AreMatched(int firstUserId, int secondUserId);
    }
}
