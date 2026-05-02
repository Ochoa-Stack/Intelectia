using AutoMapper;
using Intelectia.Domain.Entities;
using Intelectia.Shared.DTOs.Auth;
using Intelectia.Shared.DTOs.Commerce;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Auth
        CreateMap<User, UserDto>()
            .ForMember(d => d.IsStudent, o => o.MapFrom(s => s.StudentProfile != null))
            .ForMember(d => d.IsVendor,  o => o.MapFrom(s => s.VendorProfile  != null));

        // Marketplace
        CreateMap<Category, CategoryDto>();

        CreateMap<Book, BookSummaryDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.Format,        o => o.MapFrom(s => s.Format.ToString()));

        CreateMap<Book, BookDetailDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.VendorName,   o => o.MapFrom(s => s.VendorProfile.BusinessName))
            .ForMember(d => d.Format,        o => o.MapFrom(s => s.Format.ToString()));

        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.UserFullName,
                o => o.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"));

        // Comercio; Carrito
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.BookTitle,     o => o.MapFrom(s => s.Book.Title))
            .ForMember(d => d.BookAuthor,    o => o.MapFrom(s => s.Book.Author))
            .ForMember(d => d.CoverImageUrl, o => o.MapFrom(s => s.Book.CoverImageUrl))
            .ForMember(d => d.Format,         o => o.MapFrom(s => s.Book.Format.ToString()));

        CreateMap<Cart, CartDto>()
            .ForMember(d => d.Items,
                o => o.MapFrom(s => s.Items.Where(i => !i.IsDeleted).ToList()));

        // Comercio; Pedidos
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.BookTitle,     o => o.MapFrom(s => s.Book.Title))
            .ForMember(d => d.BookAuthor,    o => o.MapFrom(s => s.Book.Author))
            .ForMember(d => d.CoverImageUrl, o => o.MapFrom(s => s.Book.CoverImageUrl));

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Status,
                o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.StripePaymentIntentId,
                o => o.MapFrom(s => s.Payment != null ? s.Payment.StripePaymentIntentId : null));
    }
}
