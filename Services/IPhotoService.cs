using System;
using System.Threading.Tasks;
using DatingApp.API.Entities;
using DatingApp.API.Models.Photos;

namespace DatingApp.API.Services
{
    public interface IPhotoService
    {
        Task<PhotoResponse> GetById(int id);

        Task<PhotoResponse> Upload(int userId, UploadRequest model);
    }
}
