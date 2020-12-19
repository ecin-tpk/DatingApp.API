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
    #region Interface
    public interface IPhotoService
    {
        Task<Photo> GetById(int id);
        Task<PhotoResponse> SavePhotoUrl(int userId, UploadRequest model);
        Task ChangeOrder(int userId, int photoId, byte order);
        Task<PhotoResponse> Upload(int userId, UploadRequest model);
        //Task SetMain(int userId, int photoId);
        Task Delete(int userId, int photoId);
        Task<Photo> GetMainPhoto(int userId);
    }
    #endregion

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

        // Save photo url
        public async Task<PhotoResponse> SavePhotoUrl(int userId, UploadRequest model)
        {
            var userInDb = await _userService.GetUserWithPhotos(userId);
            if (userInDb.Photos.Count == 9)
            {
                throw new AppException("You cannot have more than 9 photos");
            }

            var photo = _mapper.Map<Photo>(model);
            photo.Order = Convert.ToByte(userInDb.Photos.Count);
            //if (!userInDb.Photos.Any(u => u.IsMain))
            //{
            //    photo.IsMain = true;
            //}

            userInDb.Photos.Add(photo);

            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<PhotoResponse>(photo);
            }

            throw new AppException("Save photo url failed");
        }

        // Change photo order
        public async Task ChangeOrder(int userId, int photoId, byte order)
        {
            var photoInDb = await _context.Photos.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == photoId);
            if (photoInDb == null)
            {
                throw new AppException("User doesn't have this photo");
            }
            if (photoInDb.Order == order)
            {
                throw new AppException("Photo is already ordered");
            }

            var restPhotos = _context.Photos.Where(p => p.UserId == userId && p.Id != photoId);

            if (photoInDb.Order > order)
            {
                restPhotos = restPhotos.Where(p => p.Order >= order && p.Order < photoInDb.Order);
                foreach (var photo in restPhotos)
                {
                    photo.Order++;
                }
            }
            if (photoInDb.Order < order)
            {
                restPhotos = restPhotos.Where(p => p.Order > photoInDb.Order && p.Order <= order);
                foreach (var photo in restPhotos)
                {
                    photo.Order--;
                }
            }

            _context.Photos.UpdateRange(restPhotos);

            photoInDb.Order = order;
            _context.Photos.Update(photoInDb);

            await _context.SaveChangesAsync();
        }

        // Upload image to Cloudinary
        public async Task<PhotoResponse> Upload(int userId, UploadRequest model)
        {
            var userInDb = await _userService.GetUserWithPhotos(userId);
            if (userInDb.Photos.Count == 9)
            {
                throw new AppException("You cannot have more than 9 photos");
            }

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

            if (uploadResult.Url != null)
            {
                model.Url = uploadResult.Url.ToString();
                model.PublicId = uploadResult.PublicId;
            }

            var photo = _mapper.Map<Photo>(model);
            photo.Order = Convert.ToByte(userInDb.Photos.Count);

            //if (!userInDb.Photos.Any(u => u.IsMain))
            //{
            //    photo.IsMain = true;
            //}

            userInDb.Photos.Add(photo);

            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<PhotoResponse>(photo);
            }

            throw new AppException("Upload failed");
        }

        //// Set a photo as main photo
        //public async Task SetMain(int userId, int photoId)
        //{
        //    var userInDb = await _userService.GetUser(userId);

        //    if (!userInDb.Photos.Any(p => p.Id == photoId))
        //    {
        //        throw new AppException("Could not find a photo with given id");
        //    }

        //    var photoInDb = await GetById(photoId);

        //    if (photoInDb.IsMain)
        //    {
        //        throw new AppException("This is already the main photo");
        //    }

        //    var currentMainPhoto = await GetMainPhoto(userId);

        //    currentMainPhoto.IsMain = false;

        //    photoInDb.IsMain = true;

        //    await _context.SaveChangesAsync();
        //}

        // Delete photo
        public async Task Delete(int userId, int photoId)
        {
            if (await _context.Photos.AnyAsync(p => p.UserId == userId && p.Id == photoId) == false)
            {
                throw new AppException("User doesn't have such a photo with given id");
            }

            if (await _context.Photos.Where(p => p.UserId == userId).CountAsync() == 1)
            {
                throw new AppException("User must have at least one photo");
            }

            var photoInDb = await GetById(photoId);

            // Delete photo on cloud if it is on cloud
            if (photoInDb.PublicID != null)
            {
                var deleteParams = new DeletionParams(photoInDb.PublicID);

                _cloudinary.Destroy(deleteParams);

                //var result = _cloudinary.Destroy(deleteParams);
                //if (result.Result != "ok")
                //{
                //    _context.Remove(photoInDb);
                //}
            }

            _context.Remove(photoInDb);

            var restPhotos = _context.Photos.Where(p =>
                p.UserId == userId &&
                p.Id != photoId &&
                p.Order > photoInDb.Order);
            if (await restPhotos.CountAsync() > 0)
            {
                foreach (var photo in restPhotos)
                {
                    photo.Order--;
                }
            }

            _context.Photos.UpdateRange(restPhotos);

            await _context.SaveChangesAsync();
        }

        // Get the main photo by userId
        public async Task<Photo> GetMainPhoto(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).SingleOrDefaultAsync(p => p.Order == 0);
        }
    }
}
