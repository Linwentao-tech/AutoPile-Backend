using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;

namespace AutoPile.API.Validators
{
    public class OrderRequestValidator : AbstractValidator<OrderCreateDTO>
    {
        private bool BeValidPaymentMethod(string paymentMethod)
        {
            var validMethods = new[] { "Credit Card", "PayPal", "Stripe" };
            return validMethods.Contains(paymentMethod);
        }

        public OrderRequestValidator()
        {
            RuleFor(x => x.UserId)
               .NotEmpty().WithMessage("User ID is required")
               .MaximumLength(450).WithMessage("User ID cannot exceed 450 characters");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required")
                .MaximumLength(50).WithMessage("Payment method cannot exceed 50 characters")
                .Must(BeValidPaymentMethod).WithMessage("Invalid payment method. Must be one of: Credit Card, PayPal, Stripe");

            // Shipping Address Validation
            RuleFor(x => x.ShippingAddress_Line1)
                .NotEmpty().WithMessage("Shipping address line 1 is required")
                .MaximumLength(100).WithMessage("Address line 1 cannot exceed 100 characters");

            RuleFor(x => x.ShippingAddress_Line2)
                .MaximumLength(100).WithMessage("Address line 2 cannot exceed 100 characters");

            RuleFor(x => x.ShippingAddress_City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

            RuleFor(x => x.ShippingAddress_Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

            RuleFor(x => x.ShippingAddress_State)
                .NotEmpty().WithMessage("State is required")
                .MaximumLength(100).WithMessage("State cannot exceed 100 characters");

            RuleFor(x => x.ShippingAddress_PostalCode)
                .NotEmpty().WithMessage("Postal code is required")
                .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters")
                .Matches(@"^[A-Za-z0-9\s-]+$").WithMessage("Postal code can only contain letters, numbers, spaces, and hyphens");
        }
    }
}