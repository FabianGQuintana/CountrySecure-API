
using AutoMapper;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Properties;

namespace CountrySecure.Application.MappingProfiles
{
    // Hereda de Profile de AutoMapper
    public class PropertyProfile : Profile
    {
        public PropertyProfile()
        {
            // 1. DTO de ENTRADA (Creación)
            // Mapea del DTO (lo que entra por la API) a la Entidad (lo que se guarda en la BD)
            CreateMap<CreatePropertyDto, Property>()
                // Ignora el IdProperty para la creación, ya que es autogenerado.
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                // Puedes establecer valores por defecto aquí
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.PropertyStatus.Available));


            // 2. DTO de SALIDA (Consulta)
            // Mapea de la Entidad (lo que viene de la BD) al DTO (lo que se expone por la API)
            CreateMap<Property, PropertyResponseDto>();
            // Si PropertyDto tiene 'OwnerFullName', puedes hacer un mapeo más complejo:
            // .ForMember(dest => dest.OwnerFullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}")); 
            // (Esto requiere que cargues la Entidad User en la consulta, pero es el concepto)
        }
    }
}