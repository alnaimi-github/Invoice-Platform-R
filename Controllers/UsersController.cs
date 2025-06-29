using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InvoiceProcessing.API.DTOs.Auth;
using InvoiceProcessing.API.DTOs.Invoices;
using InvoiceProcessing.API.DTOs.Users;
using InvoiceProcessing.API.Services.Interfaces;

namespace InvoiceProcessing.API.Controllers;

public class UsersController(IUserService userService) : BaseController
{
    [HttpPost(ApiEndpoints.Users.Register)]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var user = await userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost(ApiEndpoints.Users.Login)]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var response = await userService.LoginAsync(loginDto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet(ApiEndpoints.Users.GetUser)]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Users can only view their own profile unless they're admin
            if (currentUserId != id && currentUserRole != "Admin")
            {
                return Forbid();
            }

            var user = await userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet(ApiEndpoints.Users.GetUsers)]
    //[Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var users = await userService.GetUsersAsync(page, pageSize);
        return Ok(users);
    }

    [HttpPut(ApiEndpoints.Users.UpdateUser)]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UserDto userDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Users can only update their own profile unless they're admin
            if (currentUserId != id && currentUserRole != "Admin")
            {
                return Forbid();
            }

            var updatedUser = await userService.UpdateUserAsync(id, userDto);
            return Ok(updatedUser);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost(ApiEndpoints.Users.VerifyEmail)]
    [Authorize]
    public async Task<ActionResult> VerifyEmail(Guid id, [FromBody] VerificationTokenDto tokenDto)
    {
        var success = await userService.VerifyEmailAsync(id, tokenDto.Token);
        if (success)
        {
            return Ok(new { message = "Email verified successfully" });
        }
        return BadRequest(new { message = "Invalid verification token" });
    }

    [HttpPost(ApiEndpoints.Users.VerifyPhone)]
    [Authorize]
    public async Task<ActionResult> VerifyPhone(Guid id, [FromBody] VerificationCodeDto codeDto)
    {
        var success = await userService.VerifyPhoneAsync(id, codeDto.Code);
        if (success)
        {
            return Ok(new { message = "Phone verified successfully" });
        }
        return BadRequest(new { message = "Invalid verification code" });
    }

    [HttpPatch(ApiEndpoints.Users.UpdateVerificationStatus)]
    //[Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateVerificationStatus(Guid id, [FromBody] UpdateVerificationStatusDto statusDto)
    {
        try
        {
            await userService.UpdateVerificationStatusAsync(id, statusDto.Status);
            return Ok(new { message = "Verification status updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}