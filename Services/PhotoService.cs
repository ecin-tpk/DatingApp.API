using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Photos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        private readonly IUserService _userService;

        private readonly IOptions<CloudinarySettings> _cloudinarySettings;

        private Cloudinary _cloudinary;

        public PhotoService(DataContext context, IMapper mapper, IUserService userService, IOptions<CloudinarySettings> cloudinarySettings)
        {
            _context = context;

            _mapper = mapper;

            _userService = userService;

            _cloudinarySettings = cloudinarySettings;

            Account account = new Account(_cloudinarySettings.Value.CloudName, _cloudinarySettings.Value.ApiKey, _cloudinarySettings.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }

        // Get photo by id
        public async Task<Photo> GetById(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        }

        // Upload image to Cloudinary
        public async Task<PhotoResponse> Upload(int userId, UploadRequest model)
        {
            var userInDb = await _userService.GetUser(userId);

            var uploadResult = new ImageUploadResult();

            var file = model.File;
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            model.Url = uploadResult.Url.ToString();
            model.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(model);

            if (!userInDb.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
            }

            userInDb.Photos.Add(photo);

            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<PhotoResponse>(photo);
            }

            throw new AppException("Upload failed");
        }

        // Set a photo as main photo
        public async Task SetMain(int userId, int photoId)
        {
            var userInDb = await _userService.GetUser(userId);

            if (!userInDb.Photos.Any(p => p.Id == photoId))
            {
                throw new AppException("Could not find a photo with given id");
            }

            var photoInDb = await GetById(photoId);

            if (photoInDb.IsMain)
            {
                throw new AppException("This is already the main photo");
            }

            var currentMainPhoto = await GetMainPhoto(userId);

            currentMainPhoto.IsMain = false;

            photoInDb.IsMain = true;

            await _context.SaveChangesAsync();
        }

        // Delete photo
        public async Task Delete(int userId, int photoId)
        {
            var userInDb = await _userService.GetUser(userId);

            if (!userInDb.Photos.Any(p => p.Id == photoId))
            {
                throw new AppException("Could not find a photo with given id");
            }

            var photoInDb = await GetById(photoId);

            if (photoInDb.IsMain)
            {
                throw new AppException("Cannot delete main photo");
            }

            // Delete photo on cloud if it is on cloud
            if (photoInDb.PublicID != null)
            {
                var deleteParams = new DeletionParams(photoInDb.PublicID);

                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    _context.Remove(photoInDb);
                }
            }
            else
            {
                _context.Remove(photoInDb);
            }

            await _context.SaveChangesAsync();
        }

        // Get the main photo by userId
        public async Task<Photo> GetMainPhoto(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).SingleOrDefaultAsync(p => p.IsMain);
        }
    }
}
