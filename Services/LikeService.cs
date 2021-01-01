using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Likes;
using DatingApp.API.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Services
{
    #region Interface
    public interface ILikeService
    {
        Task<bool> LikeUser(int userId, int recipientId, bool super);
        Task<bool> GetLike(int userId, int recipientId);
        Task<bool> AreMatched(int firstUserId, int secondUserId);
        Task<IEnumerable<int>> GetUserLikes(int id, bool likers);
        Task<IEnumerable<int>> GetMatched(int userId);
        Task<IEnumerable<int>> GetLikedButNotMatched(int userId);
        Task Unmatch(int userId, int recipientId);

        Task<MatchReponse> GetMatchUserWithFcmTokens(int id);
    }
    #endregion

    public class LikeService : ILikeService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public LikeService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Like a user
        public async Task<bool> LikeUser(int userId, int recipientId, bool super)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == recipientId))
            {
                throw new AppException("Recipient not found");
            }
            if (userId == recipientId)
            {
                throw new AppException("Yes, I love myself too");
            }
            if (await GetLike(userId, recipientId))
            {
                throw new AppException("You already liked this user");
            }
            var like = new Like
            {
                LikerId = userId,
                LikeeId = recipientId,
                Super = super
            };
            _context.Add(like);
            if (await _context.SaveChangesAsync() <= 0)
            {
                throw new AppException("Failed to like user");
            }
            // If they liked me (matched) then return true
            return await GetLike(recipientId, userId);
        }

        // Return a like object if this user has liked another one
        public async Task<bool> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.AnyAsync(l => l.LikerId == userId && l.LikeeId == recipientId);
        }

        // Check if 2 users are matched
        public async Task<bool> AreMatched(int firstUserId, int secondUserId)
        {
            var firstLike = await _context.Likes.AnyAsync(l => l.LikerId == firstUserId && l.LikeeId == secondUserId);
            var secondLike = await _context.Likes.AnyAsync(l => l.LikerId == secondUserId && l.LikeeId == firstUserId);

            return (firstLike && secondLike);
        }

        // Get likes from sender or recipient
        public async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                .Include(u => u.Likers)
                .Include(u => u.Likees)
                .SingleOrDefaultAsync(u => u.Id == id);

            // If likers = true, return list of userId that i liked, otherwise return list of userId that like me
            if (likers)
            {
                return user.Likers.Where(l => l.LikeeId == id && !l.Unmatched).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(l => l.LikerId == id && !l.Unmatched).Select(i => i.LikeeId);
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
            var likees = user.Likers.Where(l => l.LikeeId == userId && !l.Unmatched).Select(l => l.LikerId);

            // Find id of users that liked me
            var likers = user.Likees.Where(l => l.LikerId == userId && !l.Unmatched).Select(l => l.LikeeId);

            // Matched users are who liked me and i already liked them
            var matchedUsers = likees.Where(i => likers.Contains(i));

            return matchedUsers;
        }

        // Unmatch
        public async Task Unmatch(int userId, int recipientId)
        {
            if (await AreMatched(userId, recipientId) == false)
            {
                throw new AppException("Users are not matched");
            }

            var like = await _context.Likes.FirstOrDefaultAsync(l => l.LikerId == userId && l.LikeeId == recipientId);
            like.Unmatched = true;

            _context.Likes.Update(like);

            if (await _context.SaveChangesAsync() == 0)
            {
                throw new AppException("Update failed");
            }
        }

        // Liked but not matched
        public async Task<IEnumerable<int>> GetLikedButNotMatched(int userId)
        {
            var likees = await GetUserLikes(userId, false);
            var matched = await GetMatched(userId);
            return likees.Where(i => !matched.Contains(i));
        }

        public async Task<MatchReponse> GetMatchUserWithFcmTokens(int id)
        {
            var photos = await _context.Photos.Where(p => p.UserId == id && p.Order == 0).Select(p => new Photo { Url = p.Url }).ToListAsync();

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new User { Id = u.Id, Name = u.Name, Photos = photos })
                .FirstAsync();

            var test = await _context.Users.FirstAsync(u => u.Id == id);

            var test2 = _mapper.Map<MatchReponse>(test);
            test2.FcmTokens = test.FcmTokens.Select(t => t.Token);
            test2.PhotoUrl = await _context.Photos.Where(p => p.UserId == id && p.Order == 0).Select(p => p.Url).FirstOrDefaultAsync();

            var mappedUser = _mapper.Map<MatchReponse>(user);

            //return mappedUser;
            return test2;
        }
    }
}
