using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Services
{
    #region Interface
    public interface IInterestService
    {
        IEnumerable<Activity> GetAll(int userId);
        Task AddInterest(int userId, int activityId);
        Task RemoveInterest(int userId, int activityId);
    }
    #endregion
    public class InterestService : IInterestService
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;
        public InterestService(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // Get all interests of user
        public IEnumerable<Activity> GetAll(int userId)
        {
            var interests = _context.Interests.Where(i => i.UserId == userId).Select(u => u.ActivityId);

            var activities = _context.Activities.Where(a => interests.Contains(a.Id));

            return activities.ToList();
        }

        // Add interest
        public async Task AddInterest(int userId, int activityId)
        {
            if (await _context.Activities.SingleOrDefaultAsync(a => a.Id == activityId) == null)
            {
                throw new KeyNotFoundException("Subject not found");
            }
            if (await _context.Interests.SingleOrDefaultAsync(i => i.UserId == userId && i.ActivityId == activityId) != null)
            {
                throw new AppException("You already had interest in this subject");
            }

            var interest = new Interest
            {
                UserId = userId,
                ActivityId = activityId
            };

            _context.Add(interest);

            await _context.SaveChangesAsync();
        }



        // Delete photo
        public async Task RemoveInterest(int userId, int activityId)
        {
            var interest = await _context.Interests.SingleOrDefaultAsync(i => i.UserId == userId && i.ActivityId == activityId);
            if (interest == null)
            {
                throw new AppException("You haven't added this interest yet");
            }

            _context.Remove(interest);

            await _context.SaveChangesAsync();
        }
    }
}
