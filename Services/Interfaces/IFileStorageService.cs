namespace InvoiceProcessing.API.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> GetFileAsync(string fileKey);
    Task<string> GetFileUrlAsync(string fileKey, TimeSpan expiry);
    Task<bool> DeleteFileAsync(string fileKey);
    Task<bool> FileExistsAsync(string fileKey);
}