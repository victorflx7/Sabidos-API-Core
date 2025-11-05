using AutoMapper;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;

public class EventoProfile : Profile
{
    public EventoProfile()
    {
        // Model → Response DTO
        CreateMap<Evento, EventoResponseDto>()
            .ForMember(dest => dest.AuthorName,
                       opt => opt.MapFrom(src => src.User != null ? src.User.Name : "Usuário"));

        // ✅ CORRIGIDO: Mapeamento explícito
        CreateMap<EventoCreateDto, Evento>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.DescriptionEvent, opt => opt.MapFrom(src => src.DescriptionEvent)) // ✅
            .ForMember(dest => dest.LocalEvento, opt => opt.MapFrom(src => src.LocalEvento));         // ✅

        // Update DTO → Model  
        CreateMap<EventoUpdateDto, Evento>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}