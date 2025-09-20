using AutoMapper;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.DTOs;

namespace SabidosAPI_Core.Profiles
{
    public class ResumoProfile : Profile
    {
        public ResumoProfile()
        {
            // Mapeamento de Resumo para ResumoResponseDto
            CreateMap<Resumo, ResumoResponseDto>();

            // Mapeamento para criação
            CreateMap<ResumoCreateUpdateDto, Resumo>()
                .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
                .ForMember(dest => dest.Conteudo, opt => opt.MapFrom(src => src.Conteudo))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                // Ignorar propriedades que serão definidas no service
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorUid, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Mapeamento para atualização - ignorar explicitamente cada propriedade não desejada
            CreateMap<ResumoCreateUpdateDto, Resumo>()
                .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
                .ForMember(dest => dest.Conteudo, opt => opt.MapFrom(src => src.Conteudo))
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