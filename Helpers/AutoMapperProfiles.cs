    using System.Linq;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Models.Account;
using DatingApp.API.Models.Admin;
using DatingApp.API.Models.Messages;
using DatingApp.API.Models.Photos;
using DatingApp.API.Models.Reports;
using DatingApp.API.Models.Users;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<NewUserRequest, User>();
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.HideAge ? null : src.DateOfBirth));
            CreateMap<User, UserDetailsResponse>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.HideAge ? null : src.DateOfBirth));
            CreateMap<User, UserForAdminResponse>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url));
            CreateMap<UpdateRequest, User>()
                .ForAllMembers(x => x.Condition((src, dest, prop) =>
                {
                    // Ignore null and empty string properties
                    if (prop == null)
                    {
                        return false;
                    }
                    if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop))
                    {
                        return false;
                    }

                    // Ignore null role
                    if (x.DestinationMember.Name == "Role" && src.Role == null)
                    {
                        return false;
                    }

                    return true;
                }));
            CreateMap<User, UpdateResponse>();

            CreateMap<RegisterRequest, User>();
            CreateMap<User, LoginResponse>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url));
            CreateMap<FacebookLoginResponse, User>();

            CreateMap<UploadRequest, Photo>();
            CreateMap<Photo, PhotoResponse>();

            CreateMap<NewMessageRequest, Message>().ReverseMap();
            CreateMap<Message, MessageResponse>()
                .ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(src => src.Recipient.Photos.SingleOrDefault(p => p.IsMain).Url));


            CreateMap<NewReportRequest, Report>().ReverseMap();
        }
    }
}
