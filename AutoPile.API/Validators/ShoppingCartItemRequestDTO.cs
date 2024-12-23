using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;

namespace AutoPile.API.Validators
{
    public class ShoppingCartItemRequestDTO : AbstractValidator<ShoppingCartItemRequestDto>
    {
        public ShoppingCartItemRequestDTO()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required.")
                .GreaterThan(0)
                .WithMessage("Product ID must be greater than 0.");

            RuleFor(x => x.Quantity)
                .NotEmpty()
                .WithMessage("Quantity is required.")
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Quantity cannot exceed 100 items per order.");
        }
    }
}