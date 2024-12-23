﻿using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;

namespace AutoPile.API.Validators
{
    public class ProductRequestValidator : AbstractValidator<ProductCreateDTO>
    {
        public ProductRequestValidator()
        {
            RuleFor(x => x.Name)
               .NotEmpty().WithMessage("Product name is required")
               .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.ProductInfo)
                .NotEmpty().WithMessage("Product info is required")
                .MaximumLength(2000).WithMessage("Product info cannot exceed 2000 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
                .Matches(@"^[A-Za-z0-9-]+$").WithMessage("SKU can only contain letters, numbers, and hyphens");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0")
                .PrecisionScale(18, 2, true).WithMessage("Price cannot have more than 2 decimal places");

            RuleFor(x => x.ComparePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Compare price must be greater than or equal to 0")
                .PrecisionScale(18, 2, true).WithMessage("Compare price cannot have more than 2 decimal places")
                .Must((model, comparePrice) => comparePrice == 0 || comparePrice > model.Price)
                .WithMessage("Compare price must be greater than regular price when specified");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0")
                .Must((model, quantity) => !model.IsInStock || quantity > 0)
                .WithMessage("Stock quantity must be greater than 0 when product is in stock");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid category value");

            RuleFor(x => x.Ribbon)
                .IsInEnum().WithMessage("Invalid ribbon value");

            RuleFor(x => x)
                .Must(x => !x.IsInStock || x.StockQuantity > 0)
                .WithMessage("Cannot mark product as in stock with zero quantity")
                .Must(x => x.IsInStock || x.StockQuantity == 0)
                .WithMessage("Out of stock products should have zero quantity");
        }
    }
}