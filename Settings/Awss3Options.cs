namespace InvoiceProcessing.API.Settings;

public sealed class Awss3Options
{
    public const string SectionName = "AWS";
    public string Profile { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public S3Options S3 { get; init; } = new();

    public sealed class S3Options
    {
        public string BucketName { get; init; } = string.Empty;
    }
}