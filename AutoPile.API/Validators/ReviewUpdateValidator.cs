using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;

namespace AutoPile.API.Validators
{
    public class ReviewUpdateValidator : AbstractValidator<ReviewUpdateDTO>
    {
        public ReviewUpdateValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Subtitle)
                .MaximumLength(500).WithMessage("Subtitle cannot exceed 500 characters");

            RuleFor(x => x.Content)
                .MinimumLength(10).WithMessage("Content must be at least 10 characters long")
                .MaximumLength(5000).WithMessage("Content cannot exceed 5000 characters");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

            RuleFor(x => x.Image)
                .Must(BeValidImage).When(x => x.Image != null)
                .WithMessage("Invalid image format. Only jpg, jpeg, png files are allowed and must be less than 5MB");
        }

        private bool BeValidImage(IFormFile file)
        {
            if (file == null) return true;

            if (file.Length > 5 * 1024 * 1024)
                return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }
    }
}