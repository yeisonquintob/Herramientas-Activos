using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Navi.ToolsAssets.Application.Documents;

namespace Navi.ToolsAssets.Infrastructure.Storage;

public sealed class MinioDocumentStorageService : IDocumentStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public MinioDocumentStorageService(IConfiguration configuration)
    {
        var endpoint = ReadConfigurationValue(configuration, "Minio:Endpoint", "localhost:9100");
        var accessKey = ReadConfigurationValue(configuration, "Minio:AccessKey", "naviadmin");
        var secretKey = ReadConfigurationValue(configuration, "Minio:SecretKey", "Navitrans_2026*Minio!");
        var useSsl = bool.TryParse(configuration["Minio:UseSsl"], out var ssl) && ssl;

        _bucketName = ReadConfigurationValue(configuration, "Minio:BucketName", "navi-tools-documents").Trim().ToLowerInvariant();

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSsl)
            .Build();
    }

    private static string ReadConfigurationValue(IConfiguration configuration, string key, string fallback)
    {
        var value = configuration[key];

        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
    }

    public async Task<string> UploadAsync(
        string objectName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

        return objectName;
    }

    public async Task<Stream> DownloadAsync(
        string objectName,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var memoryStream = new MemoryStream();

        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(memoryStream);
            });

        await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);

        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<bool> ExistsAsync(
        string objectName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            await _minioClient.StatObjectAsync(statObjectArgs, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_bucketName))
        {
            throw new InvalidOperationException("La configuración Minio:BucketName está vacía. Configure el bucket navi-tools-documents.");
        }

        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_bucketName);

        var exists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

        if (!exists)
        {
            var makeBucketArgs = new MakeBucketArgs()
                .WithBucket(_bucketName);

            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
        }
    }
}
