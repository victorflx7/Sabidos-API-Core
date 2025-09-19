using AutoMapper;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.DTOs;

namespace SabidosAPI_Core.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Mapeamento de User para UserResponseDto
            CreateMap<User, UserResponseDto>();

            // Mapeamento de UserUpdateDto para User (para atualização)
            CreateMap<UserUpdateDto, User>()
            .ForMember(dest => dest.Name, opt => opt
                .MapFrom(src => src.Name)) 
            .ForMember(dest => dest.UpdatedAt, opt => opt
                .MapFrom(_ => DateTime.UtcNow))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                srcMember != null));
                }
    }
}