using System.Threading.Tasks;
using DatingApp.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Services
{
    public class LikeService : ILikeService
    {
        private readonly DataContext _context;

        public LikeService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AreMatched(int firstUserId, int secondUserId)
        {
            var firstLike = await _context.Likes.SingleOrDefaultAsync(l => l.LikerId == firstUserId && l.LikeeId == secondUserId);
            var secondLike = await _context.Likes.SingleOrDefaultAsync(l => l.LikerId == secondUserId && l.LikeeId == firstUserId);
            if (firstLike == null || secondLike == null)
            {
                return false;
            }

            return true;
        }
    }
}
