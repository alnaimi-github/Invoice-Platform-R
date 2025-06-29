using AutoMapper;
using InvoiceProcessing.API.DTOs.Users;
using InvoiceDto = InvoiceProcessing.API.DTOs.Invoices.InvoiceDto;
using InvoiceLineDto = InvoiceProcessing.API.DTOs.Invoices.InvoiceLineDto;
using InvoiceTaxDto = InvoiceProcessing.API.DTOs.Invoices.InvoiceTaxDto;

namespace InvoiceProcessing.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, UserSummaryDto>();
        CreateMap<CreateUserDto, User>();

        CreateMap<Invoice, InvoiceDto>();
        CreateMap<InvoiceLine, InvoiceLineDto>();
        CreateMap<InvoiceTax, InvoiceTaxDto>();

        CreateMap<VerificationDocument, VerificationDocumentDto>();
    }
}

public class VerificationDocumentDto
{
    public Guid Id { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public VerificationStatus Status { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}