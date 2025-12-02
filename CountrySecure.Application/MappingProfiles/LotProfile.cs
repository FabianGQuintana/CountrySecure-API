using AutoMapper;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Lots;

namespace CountrySecure.Application.MappingProfiles
{
    // Hereda de la clase base Profile de AutoMapper
    public class LotProfile : Profile
    {
        public LotProfile()
        {
            // === 1. MAPEADOR DE ENTRADA (Creación: DTO -> Entidad) ===
            // Convierte CreateLotDto (Input) a Lot (Dominio)
            CreateMap<CreateLotDto, Lot>()
                // Ignorar el mapeo de Id, ya que se genera con Guid.NewGuid() en la Entidad Base.
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                // Si la Entidad Base tiene CreatedAt y CreatedBy, también se ignoran aquí
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // === 2. MAPEADOR DE SALIDA (Consulta: Entidad -> DTO) ===
            // Convierte Lot (Dominio) a LotResponseDto (Output para la API)
            CreateMap<Lot, LotResponseDto>();

            // === 3. MAPEADOR DE ACTUALIZACIÓN (Patch: DTO -> Entidad existente) ===
            // Convierte UpdateLotDto (Input) a Lot (Dominio).
            // Usamos una condición crítica para permitir la actualización parcial.
            CreateMap<UpdateLotDto, Lot>()
                // 🔑 REGLA CRÍTICA: Solo mapear si el valor de origen (DTO) NO es nulo.
                // Esto permite que el cliente solo envíe los campos que quiere modificar.
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember, context) => srcMember != null));

            // Si tienes un DTO de un solo campo para el Block Name (BlockNameDto)
            CreateMap<string, LotNameBlockDto>()
                .ForMember(dest => dest.NameBlock, opt => opt.MapFrom(src => src));
        }
    }
}