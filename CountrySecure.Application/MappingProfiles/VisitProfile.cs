using AutoMapper;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Visits;

namespace CountrySecure.Application.MappingProfiles
{
    public class VisitProfile : Profile
    {
        public VisitProfile()
        {
            // 1. DTO de ENTRADA (Creación)
            CreateMap<CreateVisitDto, Visit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            // Sin enums ni valores por defecto ya que Visit no usa Status

            // 2. DTO de ENTRADA (Actualización)
            CreateMap<UpdateVisitDto, Visit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // 3. DTO de SALIDA (Consulta)
            CreateMap<Visit, VisitDto>();
        }
    }
}
