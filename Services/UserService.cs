using AutoMapper;
using InvoiceProcessing.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InvoiceProcessing.API.DTOs.Auth;
using InvoiceProcessing.API.DTOs.Users;
using InvoiceProcessing.API.Services.Interfaces;

namespace InvoiceProcessing.API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public UserService(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = _mapper.Map<User>(createUserDto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        // Sellers need verification, buyers can be auto-approved for basic access
        user.VerificationStatus = createUserDto.Role == UserRole.Seller
            ? VerificationStatus.Pending
            : VerificationStatus.Approved;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            User = _mapper.Map<UserDto>(user)
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20)
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto> UpdateUserAsync(Guid userId, UserDto userDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.PhoneNumber = userDto.PhoneNumber;
        user.CompanyName = userDto.CompanyName;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> VerifyEmailAsync(Guid userId, string token)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // In a real implementation, you would validate the token
        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyPhoneAsync(Guid userId, string code)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // In a real implementation, you would validate the code
        user.IsPhoneVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateVerificationStatusAsync(Guid userId, VerificationStatus status)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        user.VerificationStatus = status;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "DefaultSecretKey123456789"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("VerificationStatus", user.VerificationStatus.ToString())
        };

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"] ?? "InvoiceProcessingAPI",
            _configuration["Jwt:Audience"] ?? "InvoiceProcessingAPI",
            claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}