using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;

namespace AutoPile.API.Validators
{
    public class OrderItemUpdateValidator : AbstractValidator<OrderItemUpdateDTO>
    {
        public OrderItemUpdateValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 items");
        }
    }
}