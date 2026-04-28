using AutoMapper;
using Intelectia.Domain.Entities;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeo de entidad User al DTO que se devuelve en las respuestas de auth
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.IsStudent, opt => opt.MapFrom(src => src.StudentProfile != null))
            .ForMember(dest => dest.IsVendor,  opt => opt.MapFrom(src => src.VendorProfile != null));
    }
}
