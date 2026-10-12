using FluentValidation;
using Resto.Api.Dtos;

namespace Resto.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Enter your username.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Enter your password.");
    }
}

public class SaveCategoryRequestValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SortOrder).InclusiveBetween(0, 999);
    }
}

public class SaveMenuItemRequestValidator : AbstractValidator<SaveMenuItemRequest>
{
    public SaveMenuItemRequestValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Choose a category.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);

        // Matches the NUMBER(8,2) column: up to 999999.99, two decimals
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be more than RM 0.")
            .LessThan(1_000_000m)
            .PrecisionScale(8, 2, true).WithMessage("Price can have at most 2 decimal places.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .Must(BeAnHttpUrl).WithMessage("Image URL must start with http:// or https://")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }

    private static bool BeAnHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public class SaveTableRequestValidator : AbstractValidator<SaveTableRequest>
{
    public SaveTableRequestValidator()
    {
        RuleFor(x => x.TableNo).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Seats).InclusiveBetween(1, 99);
    }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Username can only use letters, numbers, dots, dashes and underscores.");

        RuleFor(x => x.Password).StrongPassword();
        RuleFor(x => x.Role).IsInEnum();
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Password).StrongPassword();
    }
}

/// <summary>Shared password rule for new accounts and password resets.</summary>
internal static class PasswordRules
{
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty().WithMessage("Enter a password.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100);
}
