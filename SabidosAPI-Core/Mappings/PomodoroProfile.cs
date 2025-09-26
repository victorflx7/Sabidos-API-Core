using AutoMapper;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Dtos;

namespace SabidosAPI_Core.AutoMapper
{
    public class PomodoroProfile : Profile
    {
        public PomodoroProfile()
        {
            CreateMap<Pomodoro, PomoResponseDto>();
            CreateMap<PomoCreateDto, Pomodoro>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}