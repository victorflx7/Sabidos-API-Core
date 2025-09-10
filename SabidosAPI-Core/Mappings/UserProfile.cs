using AutoMapper;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;

namespace SabidosAPI_Core.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponseDto>();
        CreateMap<UserUpdateDto, User>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
