using System.Linq;
using AutoMapper;
using DatingApp.API.Controllers.DTOs;
using DatingApp.API.DTOs;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDTO>()
            .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url);
            })
            .ForMember(dest => dest.Age, opt => {
                opt.MapFrom(x => x.DateOfBirth.CalculateAge());
            });
            CreateMap<User, UserForDetailedDTO>()
             .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url);
            })
              .ForMember(dest => dest.Age, opt => {
                opt.MapFrom(x => x.DateOfBirth.CalculateAge());
            });
            CreateMap<Photo, PhotosForDetailedDTO>();
            CreateMap<UserForUpdateDTO, User>();
            CreateMap<UserForRegisterDTO, User>();
        }
    }
}