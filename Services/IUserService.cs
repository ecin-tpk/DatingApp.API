using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Admin;
using DatingApp.API.Models.Users;

namespace DatingApp.API.Services
{
    public interface IUserService
    {
        Task<PagedList<User>> GetPagination(UserParams userParams);

        Task<UserResponse> GetById(int id);

        Task<UserResponse> Update(int id, UpdateRequest model);

        Task<User> GetUser(int id);

        Task<int[]> GetNumberOfUsersByStatus();

        Task<UserResponse> Create(NewUserRequest model);

        Task<int[]> GetNewUsersPerMonth(int year);
    }
}
