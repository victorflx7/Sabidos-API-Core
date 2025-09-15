using AutoMapper;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.DTOs;

namespace SabidosAPI_Core.Mappings
{
    public class ResumoProfile : Profile
    {
        public ResumoProfile()
        {
            CreateMap<Resumo, ResumoResponseDto>();
            CreateMap<ResumoCreateUpdateDto, Resumo>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Resumo, ResumoCreateUpdateDto>();
        }
    }
}   

