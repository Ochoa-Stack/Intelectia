using AutoMapper;
using Intelectia.Domain.Entities;
using Intelectia.Shared.DTOs.Auth;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeo de User al DTO de respuesta de auth
        CreateMap<User, UserDto>()
            .ForMember(d => d.IsStudent, o => o.MapFrom(s => s.StudentProfile != null))
            .ForMember(d => d.IsVendor,  o => o.MapFrom(s => s.VendorProfile  != null));

        // Mapeo de Category al DTO de listado
        CreateMap<Category, CategoryDto>();

        // Mapeo de Book al resumen para tarjetas del catálogo
        CreateMap<Book, BookSummaryDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.Format,        o => o.MapFrom(s => s.Format.ToString()));

        // Mapeo de Book al detalle completo
        CreateMap<Book, BookDetailDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.VendorName,   o => o.MapFrom(s => s.VendorProfile.BusinessName))
            .ForMember(d => d.Format,        o => o.MapFrom(s => s.Format.ToString()));

        // Mapeo de Review al DTO de reseña
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.UserFullName,
                o => o.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"));
    }
}
