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
            var users = _context.Users.Include(p => p.Photos).AsQueryable();
            users = users.Where(
                u => u.Id != userParams.UserId &&
                u.Role != Role.Admin
            );
            users = users.Where(u => u.Status == userParams.Status);

            if (userParams.IsMatched)
            {
                var matchedUsers = await _likeService.GetMatched(userParams.UserId);
                users = users.Where(u => matchedUsers.Contains(u.Id));

                //users = (IQueryable<User>)_mapper.Map<IQueryable<MatchedUserResponse>>(users);
            }
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
            if (userParams.Likers)
            {
                var userLikers = await _likeService.GetUserLikes(userParams.UserId, true);

                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if (userParams.Likees)
            {
                var userLikees = await _likeService.GetUserLikes(userParams.UserId, false);

                users = users.Where(u => userLikees.Contains(u.Id));
            }
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

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
                    users = users.OrderByDescending(u => u.LastActive);
                    break;
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

            var newUsersPerMonth = new int[12];

            for (int i = 0; i < 12; i++)
            {
                newUsersPerMonth[i] = await users.Where(u => u.Created.Month == i + 1).CountAsync();
            }

            return newUsersPerMonth;
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

        // Hash password
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Get the ids of users with role admin
        public List<int> GetAdminIds()
        {
            return _context.Users.Where(u => u.Role == Role.Admin).Select(u => u.Id).ToList();
        }
    }
}
