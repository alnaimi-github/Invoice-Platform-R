using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InvoiceProcessing.API.DTOs.Invoices;
using InvoiceProcessing.API.Services.Interfaces;

namespace InvoiceProcessing.API.Controllers;

//[Authorize]
public class InvoicesController(IInvoiceService invoiceService) : BaseController
{
    [HttpPost(ApiEndpoints.Invoices.CreateInvoice)]
    //[Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromForm] CreateInvoiceDto createInvoiceDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var invoice = await invoiceService.CreateInvoiceAsync(createInvoiceDto, currentUserId);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet(ApiEndpoints.Invoices.GetInvoice)]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
    {
        try
        {
            var invoice = await invoiceService.GetInvoiceByIdAsync(id);

            var currentUserId = GetCurrentUserId();
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if user has access to this invoice
            if (currentUserRole != "Admin" &&
                invoice.Supplier.Id != currentUserId &&
                invoice.Customer.Id != currentUserId)
            {
                return Forbid();
            }

            return Ok(invoice);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet(ApiEndpoints.Invoices.GetInvoices)]
    public async Task<ActionResult<List<InvoiceDto>>> GetInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Admin can see all invoices, others only their own
        var userId = currentUserRole == "Admin" ? (Guid?)null : currentUserId;

        var invoices = await invoiceService.GetInvoicesAsync(userId, page, pageSize);
        return Ok(invoices);
    }

    [HttpPatch(ApiEndpoints.Invoices.UpdateInvoiceStatus)]
   // [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InvoiceDto>> UpdateInvoiceStatus(Guid id, [FromBody] UpdateInvoiceStatusDto statusDto)
    {
        try
        {
            var invoice = await invoiceService.UpdateInvoiceStatusAsync(id, statusDto.Status);
            return Ok(invoice);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet(ApiEndpoints.Invoices.GetInvoiceFile)]
    public async Task<ActionResult> GetInvoiceFile(Guid id)
    {
        try
        {
            var invoice = await invoiceService.GetInvoiceByIdAsync(id);

            var currentUserId = GetCurrentUserId();
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if user has access to this invoice
            if (currentUserRole != "Admin" &&
                invoice.Supplier.Id != currentUserId &&
                invoice.Customer.Id != currentUserId)
            {
                return Forbid();
            }

            var fileUrl = await invoiceService.GetInvoiceFileUrlAsync(id);
            return Ok(new { fileUrl });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete(ApiEndpoints.Invoices.DeleteInvoice)]
   // [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteInvoice(Guid id)
    {
        var success = await invoiceService.DeleteInvoiceAsync(id);
        if (success)
        {
            return NoContent();
        }
        return NotFound();
    }

}