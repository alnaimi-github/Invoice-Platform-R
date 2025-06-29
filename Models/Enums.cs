namespace InvoiceProcessing.API.Models;

public enum UserRole
{
    Admin = 1,
    Seller = 2,
    Buyer = 3
}

public enum VerificationStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    UnderReview = 4
}

public enum InvoiceStatus
{
    Draft = 1,
    Submitted = 2,
    Processed = 3,
    Approved = 4,
    Rejected = 5,
    Archived = 6
}

public enum DocumentType
{
    CommercialRegistration = 1,
    TaxCertificate = 2,
    NationalId = 3,
    PowerOfAttorney = 4,
    BankLetter = 5
}

public enum ExportFormat
{
    Xml = 1,
    Excel = 2,
    Csv = 3,
    Pdf = 4,
    Json = 5
}