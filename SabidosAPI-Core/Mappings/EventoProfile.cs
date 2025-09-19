using AutoMapper;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.DTOs;

namespace SabidosAPI_Core.Mappings
{
    public class EventoProfile : Profile
    {
        public EventoProfile()
        {
            // Model -> ResponseDto
            CreateMap<Evento, EventoResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.TitleEvent, opt => opt.MapFrom(src => src.TitleEvent ?? string.Empty))
                .ForMember(dest => dest.DataEvento, opt => opt.MapFrom(src => src.DataEvento.HasValue ? src.DataEvento.Value : default));

            // CreateDto -> Model
            CreateMap<EventoCreateDto, Evento>()
                .ForMember(dest => dest.TitleEvent, opt => opt.MapFrom(src => src.TitleEvent ?? string.Empty))
                .ForMember(dest => dest.DataEvento, opt => opt.MapFrom(src => src.DataEvento))
                .ForMember(dest => dest.AuthorUid, opt => opt.MapFrom(src => src.AuthorUid));
        }
    }
}
