using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        public UserService(DataContext contenxt, IMapper mapper)
        {
            _context = contenxt;

            _mapper = mapper;
        }

        // Get users (paginated)
        public async Task<PagedList<User>> GetPagination(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Status == userParams.Status);

            if (!string.IsNullOrEmpty(userParams.Name))
            {
                users = users.Where(u => u.Name.ToLower().Contains(userParams.Name));
            }
            if (!string.IsNullOrEmpty(userParams.Verification))
            {
                users = users.Where(u => userParams.Verification == "true" ? u.Verified.HasValue : !u.Verified.HasValue);
            }
            if (!string.IsNullOrEmpty(userParams.Gender))
            {
                users = users.Where(u => u.Gender == userParams.Gender);
            }
            //if (userParams.Likers)
            //{
            //    var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);

            //    users = users.Where(u => userLikers.Contains(u.Id));
            //}
            //if (userParams.Likees)
            //{
            //    var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);

            //    users = users.Where(u => userLikees.Contains(u.Id));
            //}
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

        // Get user by Id
        public async Task<UserResponse> GetById(int id)
        {
            var user = await GetUser(id);

            return _mapper.Map<UserResponse>(user);
        }

        // Update user info
        public async Task<UserResponse> Update(int id, UpdateRequest model)
        {
            var userFromRepo = await GetUser(id);

            // Validate on update email
            if (userFromRepo.Email != model.Email && await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                throw new AppException($"Email '{model.Email}' is already taken");
            }

            _mapper.Map(model, userFromRepo);

            userFromRepo.Updated = DateTime.Now;

            _context.Users.Update(userFromRepo);

            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<UserResponse>(userFromRepo);
            }

            throw new AppException("Update failed");
        }

        // Get full information of a user by id
        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == id);
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
    }
}
