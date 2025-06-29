using InvoiceProcessing.API.DTOs.Users;

namespace InvoiceProcessing.API.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}