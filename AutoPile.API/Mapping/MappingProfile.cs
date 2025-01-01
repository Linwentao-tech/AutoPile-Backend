using AutoMapper;
using AutoPile.API.Validators;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace AutoPile.API.Mapping
{
    public class MappingProfile : Profile
    {
        public byte[] ConvertFromFiletoArray(IFormFile Image)
        {
            if (Image == null || Image.Length == 0) return null;
            using var ms = new MemoryStream();
            Image.CopyTo(ms);
            return ms.ToArray();
        }

        public MappingProfile()
        {
            CreateMap<ReviewCreateDTO, Review>()
                .ForMember(dest => dest.ImageContentType, opt => opt.MapFrom(src => src.Image != null ? src.Image.ContentType : null))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => ConvertFromFiletoArray(src.Image)));
            CreateMap<Review, ReviewResponseDTO>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src =>
                    src.Image != null ? new ReviewImageDTO
                    {
                        Image = src.Image,
                        ImageContentType = src.ImageContentType,
                    } : null));
            CreateMap<ProductMediaCreateDTO, ProductMedia>();
            CreateMap<ProductMedia, ProductMediaResponseDTO>();
            CreateMap<ProductCreateDTO, Product>();
            CreateMap<Product, ProductResponseDTO>();
            CreateMap<OrderItemCreateDTO, OrderItem>();
            CreateMap<OrderItem, OrderItemResponseDTO>();
            CreateMap<OrderCreateDTO, Order>();
            CreateMap<Order, OrderResponseDTO>();

            CreateMap<UserSignupDTO, ApplicationUser>();
            CreateMap<UserSigninDTO, ApplicationUser>();

            CreateMap<ApplicationUser, UserResponseDTO>();
            CreateMap<UserUpdateInfoDTO, ApplicationUser>();
            CreateMap<ApplicationUser, UserInfoResponseDTO>();
            CreateMap<ShoppingCartItemRequestDto, ShoppingCartItem>();
            CreateMap<ShoppingCartItem, ShoppingCartItemResponseDTO>();
            CreateMap<UpdateShoppingCartItemDto, ShoppingCartItem>();
            CreateMap<ReviewUpdateDTO, Review>()
                .ForMember(dest => dest.ImageContentType, opt => opt.MapFrom(src => src.Image != null ? src.Image.ContentType : null))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => ConvertFromFiletoArray(src.Image))); ;
            CreateMap<ProductMediaUpdateDto, ProductMedia>();
            CreateMap<OrderItemUpdateDTO, OrderItem>();
            CreateMap<OrderUpdateValidator, Order>();
        }
    }
}