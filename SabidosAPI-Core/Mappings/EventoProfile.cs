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
            // 🔑 CORRIGIDO: Removido o .ForMember para Id, permitindo mapeamento automático (int para int)
            CreateMap<Evento, EventoResponseDto>()
                // Se EventoResponseDto.Id for INT, remova o mapeamento manual do Id ou remova o .ToString()
                .ForMember(dest => dest.TitleEvent, opt => opt.MapFrom(src => src.TitleEvent ?? string.Empty))
                .ForMember(dest => dest.DataEvento, opt => opt.MapFrom(src => src.DataEvento.HasValue ? src.DataEvento.Value : default));

            // CreateDto -> Model (Este mapeamento estava OK)
            CreateMap<EventoCreateDto, Evento>()
                .ForMember(dest => dest.TitleEvent, opt => opt.MapFrom(src => src.TitleEvent ?? string.Empty))
                .ForMember(dest => dest.DataEvento, opt => opt.MapFrom(src => src.DataEvento))
                .ForMember(dest => dest.AuthorUid, opt => opt.MapFrom(src => src.AuthorUid));
        }
    }
}