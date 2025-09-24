using AutoMapper;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.DTOs;

namespace SabidosAPI_Core.Mappings
{
    public class FlashcardProfile : Profile
    {
        public FlashcardProfile()
        {
            // Mapeamento de Flashcard para FlashcardResponseDto
            CreateMap<Flashcard, FlashcardResponseDto>();

            // Mapeamento para criação
            CreateMap<FlashcardCreateUpdateDto, Flashcard>()
                .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
                .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                .ForMember(dest => dest.Verso, opt => opt.MapFrom(src => src.Verso))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                // Ignorar propriedades que serão definidas no service
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorUid, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Mapeamento para atualização - ignorar explicitamente cada propriedade não desejada
            CreateMap<FlashcardCreateUpdateDto, Flashcard>()
                    .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
                    .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                    .ForMember(dest => dest.Verso, opt => opt.MapFrom(src => src.Verso))
                    .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                    // Ignorar explicitamente todas as outras propriedades
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.AuthorUid, opt => opt.Ignore())
                    .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
