using AutoMapper;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.DTOs;
using System;

namespace SabidosAPI_Core.Mappings
{
    public class FlashcardProfile : Profile
    {
        public FlashcardProfile()
        {
            // Mapeamento de Flashcard para FlashcardResponseDto
            CreateMap<Flashcard, FlashcardResponseDto>();

            // Mapeamento para criação (FlashcardCreateUpdateDto -> Flashcard)
            CreateMap<FlashcardCreateUpdateDto, Flashcard>()
                .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
                .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                .ForMember(dest => dest.Verso, opt => opt.MapFrom(src => src.Verso))
                // O serviço definirá CreatedAt/UpdatedAt, AuthorUid e AuthorName
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorUid, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Mapeamento para atualização (FlashcardCreateUpdateDto -> Flashcard)
            CreateMap<FlashcardCreateUpdateDto, Flashcard>()
                    .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
                    .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                    .ForMember(dest => dest.Verso, opt => opt.MapFrom(src => src.Verso))
                    .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                    // Ignorar explicitamente propriedades que não devem ser alteradas na atualização
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.AuthorUid, opt => opt.Ignore())
                    .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}