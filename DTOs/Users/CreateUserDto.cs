namespace InvoiceProcessing.API.DTOs.Users;

public sealed record CreateUserDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserRole Role,
    string? CommercialRegistrationNumber,
    string? TaxId,
    string? CompanyName
);