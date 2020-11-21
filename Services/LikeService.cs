using System.Collections.Generic;
using System.Linq;
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
        Task<IEnumerable<int>> GetUserLikes(int id, bool likers);
        Task<IEnumerable<int>> GetMatched(int userId);
    }
    #endregion

    public class LikeService : ILikeService
    {
        private readonly DataContext _context;

        public LikeService(DataContext context)
        {
            _context = context;
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
            if (await _context.Users.SingleOrDefaultAsync(u => u.Id == recipientId) == null)
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

        // Get likes from sender or recipient
        public async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                .Include(u => u.Likers)
                .Include(u => u.Likees)
                .SingleOrDefaultAsync(u => u.Id == id);

            // Get likers = true, return list of userId that i liked, otherwise return list of userId that like me
            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        // Get matched userIds
        public async Task<IEnumerable<int>> GetMatched(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Likers)
                .Include(u => u.Likees)
                .SingleOrDefaultAsync(u => u.Id == userId);

            // Find id of users that i liked
            var likees = user.Likers.Where(u => u.LikeeId == userId).Select(l => l.LikerId).ToList();

            // Find id of users that liked me
            var likers = user.Likees.Where(u => u.LikerId == userId).Select(l => l.LikeeId).ToList();

            // Matched users are who liked me and i already liked them
            var matchedUsers = likees.Where(i => likers.Contains(i));

            return matchedUsers;
        }
    }
}
