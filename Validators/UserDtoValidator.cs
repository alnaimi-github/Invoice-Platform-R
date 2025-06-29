using FluentValidation;
using InvoiceProcessing.API.DTOs.Users;

namespace InvoiceProcessing.API.Validators;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role.");

        RuleFor(x => x.VerificationStatus)
            .IsInEnum().WithMessage("Invalid verification status.");

        RuleFor(x => x.CommercialRegistrationNumber)
            .NotEmpty().When(x => !string.IsNullOrEmpty(x.CompanyName))
            .WithMessage("Commercial Registration Number is required when Company Name is provided.");

        RuleFor(x => x.TaxId)
            .NotEmpty().When(x => !string.IsNullOrEmpty(x.CompanyName))
            .WithMessage("Tax ID is required when Company Name is provided.");
    }
}