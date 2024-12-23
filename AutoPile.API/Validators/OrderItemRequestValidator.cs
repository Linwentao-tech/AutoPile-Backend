using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;

namespace AutoPile.API.Validators
{
    public class OrderItemRequestValidator : AbstractValidator<OrderItemCreateDTO>
    {
        public OrderItemRequestValidator()
        {
            RuleFor(x => x.ProductId)
              .GreaterThan(0).WithMessage("Product ID must be greater than 0");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

            RuleFor(x => x.ProductPrice)
                .GreaterThan(0).WithMessage("Product price must be greater than 0")
                .PrecisionScale(18, 2, true).WithMessage("Product price cannot have more than 2 decimal places");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 items");
        }
    }
}