using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;
using MongoDB.Bson;

namespace AutoPile.API.Validators
{
    public class ShoppingCartItemValidator : AbstractValidator<ShoppingCartItemRequestDto>
    {
        public ShoppingCartItemValidator()
        {
            RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid MongoDB ObjectId format for Product ID");

            RuleFor(x => x.Quantity)
                .NotEmpty()
                .WithMessage("Quantity is required.")
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Quantity cannot exceed 100 items per order.");
        }

        private bool BeValidObjectId(string productId)
        {
            return !string.IsNullOrEmpty(productId) && ObjectId.TryParse(productId, out _);
        }
    }

    public class UpdateShoppingCartItemValidator : AbstractValidator<UpdateShoppingCartItemDto>
    {
        public UpdateShoppingCartItemValidator()
        {
            RuleFor(x => x.Quantity)
                .NotEmpty()
                .WithMessage("Quantity is required.")
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Quantity cannot exceed 100 items.");
        }
    }
}