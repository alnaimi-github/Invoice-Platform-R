using Microsoft.AspNetCore.Mvc;
using InvoiceProcessing.API.Models;
using System.Security.Claims;
using InvoiceProcessing.API.DTOs.Invoices;
using InvoiceProcessing.API.Services.Interfaces;

namespace InvoiceProcessing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ExportController(IExportService exportService) : ControllerBase
{
    [HttpPost("invoices")]
    public async Task<ActionResult> ExportInvoices([FromBody] ExportRequestDto request)
    {
        try
        {
            var currentUserId = new Guid("3ace70be-45c9-4ed8-b014-42c4560815c6");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Admin can export all invoices, others only their own
            // var userId = currentUserRole == "Admin" ? (Guid?)null : currentUserId;
              var userId = new Guid("3ace70be-45c9-4ed8-b014-42c4560815c6");

            var result = await exportService.ExportInvoicesAsync(request, userId);

            return File(result.Data, result.ContentType, result.FileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during export", details = ex.Message });
        }
    }

    [HttpGet("invoice/{id}")]
    public async Task<ActionResult> ExportSingleInvoice(Guid id, [FromQuery] ExportFormat format = ExportFormat.Xml)
    {
        try
        {
            var result = await exportService.ExportSingleInvoiceAsync(id, format);
            return File(result.Data, result.ContentType, result.FileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Invoice not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during export", details = ex.Message });
        }
    }

    [HttpGet("invoice/{id}/xml")]
    public async Task<ActionResult> ExportInvoiceAsXml(Guid id)
    {
        return await ExportSingleInvoice(id, ExportFormat.Xml);
    }

    [HttpGet("invoice/{id}/excel")]
    public async Task<ActionResult> ExportInvoiceAsExcel(Guid id)
    {
        return await ExportSingleInvoice(id, ExportFormat.Excel);
    }

    [HttpGet("invoice/{id}/pdf")]
    public async Task<ActionResult> ExportInvoiceAsPdf(Guid id)
    {
        return await ExportSingleInvoice(id, ExportFormat.Pdf);
    }

    [HttpGet("invoice/{id}/csv")]
    public async Task<ActionResult> ExportInvoiceAsCsv(Guid id)
    {
        return await ExportSingleInvoice(id, ExportFormat.Csv);
    }

    [HttpGet("invoice/{id}/json")]
    public async Task<ActionResult> ExportInvoiceAsJson(Guid id)
    {
        return await ExportSingleInvoice(id, ExportFormat.Json);
    }

    [HttpPost("bulk")]
    public async Task<ActionResult> BulkExport([FromBody] BulkExportRequestDto request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var exportRequest = new ExportRequestDto(
                Format: request.Format,
                StartDate: null,
                EndDate: null,
                Status: null,
                SupplierId: null,
                CustomerId: null,
                InvoiceIds: request.InvoiceIds,
                IncludeLineItems: request.IncludeLineItems,
                IncludeTaxDetails: request.IncludeTaxDetails
            );

            var userId = currentUserRole == "Admin" ? (Guid?)null : currentUserId;
            var result = await exportService.ExportInvoicesAsync(exportRequest, userId);

            return File(result.Data, result.ContentType, result.FileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during bulk export", details = ex.Message });
        }
    }

    [HttpGet("formats")]
    public ActionResult GetSupportedFormats()
    {
        var formats = Enum.GetValues<ExportFormat>()
            .Select(f => new {
                Value = (int)f,
                Name = f.ToString(),
                Description = GetFormatDescription(f),
                ContentType = GetContentType(f)
            })
            .ToList();

        return Ok(formats);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    private string GetFormatDescription(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Xml => "UBL XML format for e-invoicing compliance",
            ExportFormat.Excel => "Excel spreadsheet with multiple sheets",
            ExportFormat.Csv => "Comma-separated values for data analysis",
            ExportFormat.Pdf => "PDF document for printing and sharing",
            ExportFormat.Json => "JSON format for API integration",
            _ => "Unknown format"
        };
    }

    private string GetContentType(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Xml => "application/xml",
            ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ExportFormat.Csv => "text/csv",
            ExportFormat.Pdf => "application/pdf",
            ExportFormat.Json => "application/json",
            _ => "application/octet-stream"
        };
    }
}