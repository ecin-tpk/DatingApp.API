using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.RequestParams;
using DatingApp.API.Models.Admin;
using DatingApp.API.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Services
{
    #region Interface
    public interface IUserService
    {
        Task<PagedList<User>> GetPagination(UserParams userParams);
        //Task<UserDetailsResponse> GetById(int id);

        // Simple user has id, name, first photo
        Task<SimpleUserResponse> GetSimpleUser(int id);
        Task<User> GetUserDetails(int id);

        Task<UpdateResponse> Update(int id, UpdateRequest model);
        Task<User> GetUserWithPhotos(int id);
        Task<int[]> GetNumberOfUsersByStatus();
        Task<UserResponse> Create(NewUserRequest model);
        Task<int[]> GetNewUsersPerMonth(int year);
        Task<int[]> GetTotalUsersPerMonth(int year);
        Task<int[]> CountUsersByAge(int year);
        List<int> GetAdminIds();

        Task<double> GetDistance(double latitude, double longitude, int userId);
    }
    #endregion

    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILikeService _likeService;

        public UserService(DataContext contenxt, IMapper mapper, ILikeService likeService)
        {
            _context = contenxt;
            _mapper = mapper;
            _likeService = likeService;
        }

        // Get users (paginated)
        public async Task<PagedList<User>> GetPagination(UserParams userParams)
        {
            var users = _context.Users.Include(u => u.Photos)
                .Where(u =>
                    u.Id != userParams.UserId &&
                    u.Role != Role.Admin &&
                    u.Status == userParams.Status)
                .AsQueryable();

            if (userParams.ForCards)
            {
                users = await GetUsersForCards(users, userParams);

                return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
            }
            if (userParams.IsMatched)
            {
                var matched = await _likeService.GetMatched(userParams.UserId);
                users = users.Where(u => matched.Contains(u.Id));

                return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
            }
            if (userParams.Likers)
            {
                var likers = await _likeService.GetUserLikes(userParams.UserId, true);
                var matched = await _likeService.GetMatched(userParams.UserId);
                var notMatched = likers.Where(i => !matched.Contains(i));

                users = users.Where(u => notMatched.Contains(u.Id));

                users = Sort(users, userParams);

                return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
            }
            if (userParams.Likees)
            {
                var likees = await _likeService.GetUserLikes(userParams.UserId, false);
                var matched = await _likeService.GetMatched(userParams.UserId);
                var notMatched = likees.Where(i => !matched.Contains(i));

                users = users.Where(u => notMatched.Contains(u.Id));

                users = Sort(users, userParams);

                return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
            }

            if (userParams.TopPicks)
            {
                users = GetTopPicks(users, userParams);
                ////var userLikees = await _likeService.GetUserLikes(userParams.UserId, false);
                //var dontShow = _context.Likes
                //   .Where(l =>
                //       l.LikerId == userParams.UserId ||
                //       l.LikerId == userParams.UserId && l.Unmatched)
                //   .Select(l => l.LikeeId);

                //users = users
                //    .Where(u =>
                //        u.Gender==userParams.Gender &&
                //        u.
                //        !dontShow.Contains(u.Id))
                //    .Include(u => u.Likers)
                //    .OrderByDescending(u => u.Likers.Count());

                return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
            }

            // Admin area
            if (userParams.ForAdmin)
            {
                users = GetUsersForAdmin(users, userParams);

                return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        // Get simple user
        public async Task<SimpleUserResponse> GetSimpleUser(int id)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == id))
            {
                throw new KeyNotFoundException("User not found");
            }

            var photos = await _context.Photos.Where(p => p.UserId == id && p.Order == 0).Select(p => new Photo { Url = p.Url }).ToListAsync();

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new User { Id = u.Id, Name = u.Name, Photos = photos })
                .FirstOrDefaultAsync();

            return _mapper.Map<SimpleUserResponse>(user);
        }

        // Get user by Id for normal user
        //public async Task<UserDetailsResponse> GetById(int id)
        public async Task<User> GetUserDetails(int id)
        {
            var user = await GetUserWithPhotos(id);
            if (user.Role == Role.Admin)
            {
                throw new KeyNotFoundException("User not found");
            }

            //return _mapper.Map<UserDetailsResponse>(user);          
            return user;
        }

        // Update user info
        public async Task<UpdateResponse> Update(int id, UpdateRequest model)
        {
            //var userFromRepo = await GetUserWithPhotos(id);
            var userFromRepo = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            //// Validate on update email
            //if (userFromRepo.Email != model.Email && await _context.Users.AnyAsync(u => u.Email == model.Email))
            //{
            //    throw new AppException($"Email '{model.Email}' is already taken");
            //}

            _mapper.Map(model, userFromRepo);

            userFromRepo.Updated = DateTime.Now;

            _context.Users.Update(userFromRepo);

            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<UpdateResponse>(userFromRepo);
            }

            throw new AppException("Update failed");
        }

        // Get full information of a user by id
        public async Task<User> GetUserWithPhotos(int id)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // ToList works
            user.Photos = user.Photos.OrderBy(p => p.Order).ToList();

            return user;
        }

        // Get number of users by status
        public async Task<int[]> GetNumberOfUsersByStatus()
        {
            var activeCount = await _context.Users.Where(u => u.Status == Status.Active && u.Role != Role.Admin).CountAsync();
            var disalbedCount = await _context.Users.Where(u => u.Status == Status.Disabled && u.Role != Role.Admin).CountAsync();
            var deletedCount = await _context.Users.Where(u => u.Status == Status.Deleted && u.Role != Role.Admin).CountAsync();

            return new int[] { activeCount, disalbedCount, deletedCount };
        }

        // Get number of new users permonth
        public async Task<int[]> GetNewUsersPerMonth(int year)
        {
            var users = _context.Users.Where(u => u.Created.Year == year);

            var latestMonth = await users.OrderByDescending(u => u.Created).Select(u => u.Created.Month).FirstOrDefaultAsync();

            var newUsersPerMonth = new int[latestMonth];

            for (int i = 0; i < latestMonth; i++)
            {
                newUsersPerMonth[i] = await users.Where(u => u.Created.Month == i + 1).CountAsync();
            }

            return newUsersPerMonth;
        }

        // Get number of new users permonth
        public async Task<int[]> GetTotalUsersPerMonth(int year)
        {
            var users = _context.Users.Where(u => u.Created.Year <= year);

            var latestMonth = await users.OrderByDescending(u => u.Created).Select(u => u.Created.Month).FirstOrDefaultAsync();

            var totalUsers = new int[latestMonth];

            var total = 0;

            for (int i = 0; i < latestMonth; i++)
            {
                totalUsers[i] = await users.Where(u => u.Created.Month == i + 1).CountAsync() + total;

                total += await users.Where(u => u.Created.Month == i + 1).CountAsync();
            }

            return totalUsers;
        }

        // Users by age
        public async Task<int[]> CountUsersByAge(int year)
        {
            var thisYear = DateTime.Now.Year;
            var users = _context.Users.Where(u => u.Role != Role.Admin);
            var total = await users.CountAsync();
            var young = await users.Where(u =>
                u.Created.Year <= year &&
                thisYear - u.DateOfBirth.Value.Year < 29).CountAsync();
            var old = await users.Where(u =>
                u.Created.Year <= year &&
                thisYear - u.DateOfBirth.Value.Year > 50).CountAsync();
            var counts = new int[3];
            counts[0] = young;
            counts[2] = old;
            counts[1] = total - young - old;
            return counts;
        }

        // Create new user (admin)
        public async Task<UserResponse> Create(NewUserRequest model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
                throw new AppException($"Email '{model.Email}' is already used");

            var user = _mapper.Map<User>(model);
            user.Created = DateTime.Now;
            user.Verified = DateTime.Now;

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserResponse>(user);
        }

        // Get the ids of users with role admin
        public List<int> GetAdminIds()
        {
            return _context.Users.Where(u => u.Role == Role.Admin).Select(u => u.Id).ToList();
        }

        // Get distance in met
        public async Task<double> GetDistance(double latitude, double longitude, int userId)
        {
            var myLatitude = await _context.Users.Where(u => u.Id == userId).Select(u => u.Latitude).FirstOrDefaultAsync();
            var myLongitude = await _context.Users.Where(u => u.Id == userId).Select(u => u.Longitude).FirstOrDefaultAsync();

            var d1 = myLatitude * (Math.PI / 180.0);
            var num1 = myLongitude * (Math.PI / 180.0);
            var d2 = latitude * (Math.PI / 180.0);
            var num2 = longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return Math.Round(6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)) / 1000), 2);
        }

        // Helpers

        // Get users for card stack
        private async Task<IQueryable<User>> GetUsersForCards(IQueryable<User> users, UserParams userParams)
        {
            // Don't show profiles that i liked, unmatched or reported
            var reported = _context.Reports.Where(r => r.SenderId == userParams.UserId).Select(r => r.UserId);
            var liked = _context.Likes.Where(l => l.LikerId == userParams.UserId).Select(l => l.LikeeId);
            var dontShow = liked.Union(reported);
            users = users
                .Where(u => !dontShow.Contains(u.Id));

            if (userParams.Gender == "male" || userParams.Gender == "female")
            {
                users = users.Where(u => u.Gender == userParams.Gender);
            }
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            // Check distance
            var myLatitude = await _context.Users.Where(u => u.Id == userParams.UserId).Select(u => u.Latitude).FirstOrDefaultAsync();
            var myLongitude = await _context.Users.Where(u => u.Id == userParams.UserId).Select(u => u.Longitude).FirstOrDefaultAsync();
            var inDistance = users.Select(u => new
            {
                Id = u.Id,
                Distance = CalculateDistance(u.Latitude, u.Longitude, myLatitude, myLongitude)
            }).ToList().Where(u => u.Distance <= userParams.MaxDistance * 1000).Select(u => u.Id);
            users = users.Where(u => inDistance.Contains(u.Id));

            return users.OrderBy(u => u.Created);
        }

        // Get users for admin
        private IQueryable<User> GetUsersForAdmin(IQueryable<User> users, UserParams userParams)
        {
            if (!string.IsNullOrEmpty(userParams.Name))
            {
                users = users.Where(u => u.Name.ToLower().Contains(userParams.Name));
            }
            if (!string.IsNullOrEmpty(userParams.Verification))
            {
                users = users.Where(u => userParams.Verification == "true" ? u.Verified.HasValue : !u.Verified.HasValue);
            }
            if (userParams.Gender == "male" || userParams.Gender == "female")
            {
                users = users.Where(u => u.Gender == userParams.Gender);
            }
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            return Sort(users, userParams);
        }

        // Get top picks
        private IQueryable<User> GetTopPicks(IQueryable<User> users, UserParams userParams)
        {
            // Don't show profiles that i liked, unmatched or reported
            var reported = _context.Reports.Where(r => r.SenderId == userParams.UserId).Select(r => r.UserId);
            var liked = _context.Likes
               .Where(l =>
                   l.LikerId == userParams.UserId)
               .Select(l => l.LikeeId);
            var dontShow = liked.Union(reported);

            users = users.Where(u => !dontShow.Contains(u.Id));

            if (userParams.Gender == "male" || userParams.Gender == "female")
            {
                users = users.Where(u => u.Gender == userParams.Gender);
            }
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            users = users.Include(u => u.Likers)
                .OrderByDescending(u => u.Likers.Count());

            return users;
        }

        // Sort result
        private IQueryable<User> Sort(IQueryable<User> users, UserParams userParams)
        {
            switch (userParams.OrderBy)
            {
                case "name":
                    users = users.OrderBy(u => u.Name);
                    break;
                case "gender":
                    users = users.OrderByDescending(u => u.Gender);
                    break;
                case "age":
                    users = users.OrderByDescending(u => u.DateOfBirth);
                    break;
                case "email":
                    users = users.OrderByDescending(u => u.Email);
                    break;
                case "phone":
                    users = users.OrderByDescending(u => u.Phone);
                    break;
                case "created":
                    users = users.OrderByDescending(u => u.Created);
                    break;
                case "verification":
                    users = users.OrderByDescending(u => u.Verified.HasValue);
                    break;
                default:
                    users = users.OrderBy(u => u.Created);
                    break;
            }
            return users;
        }

        // Hash password
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Calculate age
        private static int CalculateAge(DateTime? value)
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

            var dateOfBirth = Convert.ToDateTime(value);

            var leapYears = 0;

            for (int i = dateOfBirth.Year; i <= now.Year; i++)
            {
                if (DateTime.IsLeapYear(i))
                {
                    leapYears++;
                }
            }

            TimeSpan timeSpan = now.Subtract(dateOfBirth);
            return timeSpan.Days - leapYears;
        }

        private static double CalculateDistance(
            double latitude,
            double longitude,
            double myLatitude,
            double myLongitude
        )
        {
            var d1 = myLatitude * (Math.PI / 180.0);
            var num1 = myLongitude * (Math.PI / 180.0);
            var d2 = latitude * (Math.PI / 180.0);
            var num2 = longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}
