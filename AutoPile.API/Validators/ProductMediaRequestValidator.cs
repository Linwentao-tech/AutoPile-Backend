using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;

namespace AutoPile.API.Validators
{
    public static class Helper
    {
        public static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        public static bool BeValidMediaType(string mediaType)
        {
            return mediaType == "PHOTO";
        }
    }

    public class ProductMediaRequestValidator : AbstractValidator<ProductMediaCreateDTO>

    {
        public ProductMediaRequestValidator()
        {
            RuleFor(x => x.ProductId)
              .GreaterThan(0).WithMessage("Product ID must be greater than 0");

            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("URL is required")
                .MaximumLength(500).WithMessage("URL cannot exceed 500 characters");

            RuleFor(x => x.FullUrl)
                .NotEmpty().WithMessage("Full URL is required")
                .MaximumLength(2000).WithMessage("Full URL cannot exceed 2000 characters")
                .Must(Helper.BeValidUrl).WithMessage("Full URL must be a valid URL");

            RuleFor(x => x.MediaType)
                .NotEmpty().WithMessage("Media type is required")
                .MaximumLength(50).WithMessage("Media type cannot exceed 50 characters")
                .Must(Helper.BeValidMediaType).WithMessage("Media type must be : PHOTO");

            RuleFor(x => x.AltText)
                .MaximumLength(200).WithMessage("Alt text cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.AltText));

            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Width)
                .GreaterThan(0).WithMessage("Width must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Width cannot exceed 10000 pixels");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Height must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Height cannot exceed 10000 pixels");
        }
    }

    public class ProductMediaUpdateDtoValidator : AbstractValidator<ProductMediaUpdateDto>
    {
        public ProductMediaUpdateDtoValidator()
        {
            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("URL is required")
                .MaximumLength(500).WithMessage("URL cannot exceed 500 characters");

            RuleFor(x => x.FullUrl)
                .NotEmpty().WithMessage("Full URL is required")
                .MaximumLength(2000).WithMessage("Full URL cannot exceed 2000 characters")
                .Must(Helper.BeValidUrl).WithMessage("Full URL must be a valid URL");

            RuleFor(x => x.MediaType)
                .NotEmpty().WithMessage("Media type is required")
                .MaximumLength(50).WithMessage("Media type cannot exceed 50 characters")
                .Must(Helper.BeValidMediaType).WithMessage("Media type must be : PHOTO");

            RuleFor(x => x.AltText)
                .MaximumLength(200).WithMessage("Alt text cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.AltText));

            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Width)
                .GreaterThan(0).WithMessage("Width must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Width cannot exceed 10000 pixels");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Height must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Height cannot exceed 10000 pixels");
        }
    }
}