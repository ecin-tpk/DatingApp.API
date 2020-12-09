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
        Task<User> GetById(int id);
        Task<UpdateResponse> Update(int id, UpdateRequest model);
        Task<User> GetUser(int id);
        Task<int[]> GetNumberOfUsersByStatus();
        Task<UserResponse> Create(NewUserRequest model);
        Task<int[]> GetNewUsersPerMonth(int year);
        Task<int[]> GetTotalUsersPerMonth(int year);
        List<int> GetAdminIds();
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
                var userLikees = await _likeService.GetUserLikes(userParams.UserId, false);
                users = users.Where(u => !userLikees.Contains(u.Id))
                    .Include(u => u.Likers)
                    .OrderByDescending(u => u.Likers.Count());

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

        // Get user by Id for normal user
        //public async Task<UserDetailsResponse> GetById(int id)
        public async Task<User> GetById(int id)
        {
            var user = await GetUser(id);
            if (user.Role == Role.Admin)
            {
                throw new KeyNotFoundException("User not found");
            }

            // ToList works
            user.Photos = user.Photos.OrderBy(p => p.Order).ToList();

            //return _mapper.Map<UserDetailsResponse>(user);          
            return user;
        }

        // Update user info
        public async Task<UpdateResponse> Update(int id, UpdateRequest model)
        {
            var userFromRepo = await GetUser(id);

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
        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

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

        // Helpers

        // Get users for card stack
        private async Task<IQueryable<User>> GetUsersForCards(IQueryable<User> users, UserParams userParams)
        {
            // Don't show profiles that i liked
            var liked = await _likeService.GetUserLikes(userParams.UserId, false);

            //test
            //var test = users.Where(u => !liked.Contains(u.Id))
            //    .Select(u => new User
            //    {
            //        Id = u.Id,
            //        Name = u.Name,
            //        Gender = u.Gender,
            //        DateOfBirth = u.DateOfBirth,
            //        LastActive = u.LastActive,
            //        Location = u.Location,
            //        Bio = u.Bio,
            //        JobTitle = u.JobTitle,
            //        School = u.School,
            //        Company = u.Company,
            //        //Interests = u.Activities.Select(a => new
            //        //{
            //        //    a.Activity.Label
            //        //}),
            //        //Photos = u.Photos.Select(p => new
            //        //{
            //        //    p.Url
            //        //})
            //    }
            //    );

            users = users.Include(u => u.Activities)
                .ThenInclude(u => u.Activity)
                .Where(u => !liked.Contains(u.Id));


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

    }
}
