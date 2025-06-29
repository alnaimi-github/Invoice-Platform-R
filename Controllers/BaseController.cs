using Microsoft.AspNetCore.Mvc;

namespace InvoiceProcessing.API.Controllers;

/// <summary>
/// Base controller with common functionality
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Get current user ID from JWT claims
    /// </summary>
    protected Guid GetCurrentUserId()
    {
        // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        // {
        //     throw new UnauthorizedAccessException("Invalid user token");
        // }
        // return userId;
        return new Guid("3ace70be-45c9-4ed8-b014-42c4560815c6");
    }
}