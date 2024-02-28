using API.DTOs;
using API.Entites;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemeberDto>().ForMember(dest => dest.PhotoUrl,
         opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
         .ForMember(dest => dest.age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();

    }
}
