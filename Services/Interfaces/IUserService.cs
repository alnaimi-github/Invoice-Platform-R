using InvoiceProcessing.API.DTOs.Auth;
using InvoiceProcessing.API.DTOs.Users;

namespace InvoiceProcessing.API.Services.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20);
    Task<UserDto> UpdateUserAsync(Guid userId, UserDto userDto);
    Task<bool> VerifyEmailAsync(Guid userId, string token);
    Task<bool> VerifyPhoneAsync(Guid userId, string code);
    Task UpdateVerificationStatusAsync(Guid userId, VerificationStatus status);
}