using Amazon.S3.Model;
using InvoiceProcessing.API.Services.Interfaces;
using InvoiceProcessing.API.Settings;
using Microsoft.Extensions.Options;

namespace InvoiceProcessing.API.Services;

public class FileStorageService(
    IAmazonS3 s3Client,
    IOptions<Awss3Options> awss3Options) : IFileStorageService
{
    private readonly Awss3Options _awsS3Options = awss3Options.Value;

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var fileKey = $"invoices/{Guid.NewGuid()}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _awsS3Options.S3.BucketName,
            Key = fileKey,
            InputStream = fileStream,
            ContentType = contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await s3Client.PutObjectAsync(request);
        return fileKey;
    }

    public async Task<Stream> GetFileAsync(string fileKey)
    {
        var request = new GetObjectRequest
        {
            BucketName = _awsS3Options.S3.BucketName,
            Key = fileKey
        };

        var response = await s3Client.GetObjectAsync(request);
        return response.ResponseStream;
    }

    public async Task<string> GetFileUrlAsync(string fileKey, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _awsS3Options.S3.BucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        return await s3Client.GetPreSignedURLAsync(request);
    }

    public async Task<bool> DeleteFileAsync(string fileKey)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _awsS3Options.S3.BucketName,
                Key = fileKey
            };

            await s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileKey)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _awsS3Options.S3.BucketName,
                Key = fileKey
            };

            await s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}