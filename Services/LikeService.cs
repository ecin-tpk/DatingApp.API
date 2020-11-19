using System.Threading.Tasks;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Services
{
    #region Interface
    public interface ILikeService
    {
        Task LikeUser(int userId, int recipientId);
        Task<Like> GetLike(int userId, int recipientId);
        Task<bool> AreMatched(int firstUserId, int secondUserId);
    }
    #endregion

    public class LikeService : ILikeService
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public LikeService(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // Like a user
        public async Task LikeUser(int userId, int recipientId)
        {
            if (userId == recipientId)
            {
                throw new AppException("Yes, I love myself too");
            }

            if (await GetLike(userId, recipientId) != null)
            {
                throw new AppException("You already liked this user");
            }

            if (await _userService.GetUser(recipientId) == null)
            {
                throw new AppException("User not found");
            }

            var like = new Like
            {
                LikerId = userId,
                LikeeId = recipientId
            };

            _context.Add(like); 

            await _context.SaveChangesAsync();
        }

        // Return a like object if this user has liked another one
        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.SingleOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        // Check if 2 users are matched
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
