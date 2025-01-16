using AutoPile.DOMAIN.DTOs.Requests;
using FluentValidation;

namespace AutoPile.API.Validators
{
    public class UserSigninDTOValidator : AbstractValidator<UserSigninDTO>
    {
        public UserSigninDTOValidator()
        {
            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }

    public class UserSignupDTOValidator : AbstractValidator<UserSignupDTO>
    {
        public UserSignupDTOValidator()
        {
            RuleFor(u => u.UserName)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(100).WithMessage("Username must not exceed 100 characters")
                .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
                .Matches("^[a-zA-Z\\s-']+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
                .Matches("^[a-zA-Z\\s-']+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(u => u.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
                .Matches("^[0-9]{8,20}$").WithMessage("Invalid phone number format. Use a standard numeric format (e.g., 0123456789)");
        }
    }

    public class UserUpdateInfoDTOValidator : AbstractValidator<UserUpdateInfoDTO>
    {
        public UserUpdateInfoDTOValidator()
        {
            RuleFor(u => u.FirstName)
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
                .Matches("^[a-zA-Z\\s-']+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes")
                .When(u => !string.IsNullOrEmpty(u.FirstName));

            RuleFor(u => u.LastName)
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
                .Matches("^[a-zA-Z\\s-']+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes")
                .When(u => !string.IsNullOrEmpty(u.LastName));

            RuleFor(u => u.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
                .Matches("^\\+?[1-9][0-9]{7,14}$").WithMessage("Invalid phone number format. Use international format (e.g., +1234567890)")
                .When(u => !string.IsNullOrEmpty(u.PhoneNumber));
        }
    }

    public class UserResetPasswordDTOValidator : AbstractValidator<UserResetPasswordDTO>
    {
        public UserResetPasswordDTOValidator()
        {
            RuleFor(u => u.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

            RuleFor(u => u.EmailVerifyToken)
                .NotEmpty().WithMessage("Email verification token is required")
                .MaximumLength(300).WithMessage("Email verification token must not exceed 300 characters");
        }
    }
}