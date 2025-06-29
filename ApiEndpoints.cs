namespace InvoiceProcessing.API;

public static class ApiEndpoints
{
    public static class Users
    {
        private const string Base = "api/users";
        public const string Register = $"{Base}/register";
        public const string Login = $"{Base}/login";
        public const string GetUser = $"{Base}/{{id}}";
        public const string GetUsers = Base;
        public const string UpdateUser = $"{Base}/{{id}}";
        public const string VerifyEmail = $"{Base}/{{id}}/verify-email";
        public const string VerifyPhone = $"{Base}/{{id}}/verify-phone";
        public const string UpdateVerificationStatus = $"{Base}/{{id}}/verification-status";
    }

    public static class Invoices
    {
        private const string Base = "api/invoices";
        public const string CreateInvoice = Base;
        public const string GetInvoice = $"{Base}/{{id}}";
        public const string GetInvoices = Base;
        public const string UpdateInvoiceStatus = $"{Base}/{{id}}/status";
        public const string GetInvoiceFile = $"{Base}/{{id}}/file";
        public const string DeleteInvoice = $"{Base}/{{id}}";
    }

    public static class Exports
    {
        private const string Base = "api/exports";
        public const string ExportInvoices = $"{Base}/invoices";
        public const string ExportSingleInvoice = $"{Base}/invoice/{{id}}";
        public const string ExportInvoiceAsXml = $"{Base}/invoice/{{id}}/xml";
        public const string ExportInvoiceAsExcel = $"{Base}/invoice/{{id}}/excel";
        public const string ExportInvoiceAsPdf = $"{Base}/invoice/{{id}}/pdf";
        public const string ExportInvoiceAsCsv = $"{Base}/invoice/{{id}}/csv";
        public const string ExportInvoiceAsJson = $"{Base}/invoice/{{id}}/json";
        public const string BulkExport = $"{Base}/bulk";
        public const string GetSupportedFormats = $"{Base}/formats";
    }
}