namespace InvoiceProcessing.API.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserRole Role,
    VerificationStatus VerificationStatus,
    bool IsEmailVerified,
    bool IsPhoneVerified,
    string? CommercialRegistrationNumber,
    string? TaxId,
    string? CompanyName,
    DateTime CreatedAt
);