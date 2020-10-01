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
        public async Task<PhotoResponse> GetById(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return _mapper.Map<PhotoResponse>(photo);
        }

        // Upload image to Cloudinary
        public async Task<PhotoResponse> Upload(int userId, UploadRequest model)
        {
            var userFromRepo = await _userService.GetUser(userId);

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

            if (!userFromRepo.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);

            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<PhotoResponse>(photo);
            }

            throw new AppException("Upload failed");
        }
    }
}
