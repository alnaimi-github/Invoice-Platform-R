namespace InvoiceProcessing.API.Settings;

public class JwtAuthOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; init; }
    public string Issuer { get; init; }
    public string Audience { get; init; }
}