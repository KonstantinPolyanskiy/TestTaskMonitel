using Main.Application.Contracts;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Main.DAL.Minio;

public sealed class MinioStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioStorage> _logger;

    private const string _bucketName = "posters"; 

    public MinioStorage(IMinioClient minioClient, ILogger<MinioStorage> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MemoryStream> DownloadFileAsync(string key)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<string> UploadFileAsync(Stream stream, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));

        if (!await IsConnectionEstablishedAndBucketExisting(_bucketName))
        {
            await MakeBucket(_bucketName);
        }

        var key = Guid.NewGuid().ToString();
        var ct = GetContentType(fileName);

        var putArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(key)
            .WithContentType(ct)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length);

        await _minioClient.PutObjectAsync(putArgs);

        return key;
    }

    private async Task<bool> IsConnectionEstablishedAndBucketExisting(string bucketName)
    {
        var getListBucketsTask = await _minioClient.ListBucketsAsync();
        var bucketList = getListBucketsTask.Buckets;
        
        return bucketList != null && 
               bucketList.Any(el => el.Name == bucketName);
    }

    private async Task MakeBucket(string bucketName)
    {
        try
        {
            await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs()
                        .WithBucket(bucketName))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new MinioException($"Не получилось создать бакет: '{_bucketName}'. Ошибка: {ex.Message}");
        }
    }

    private static string GetContentType(string fileName, string? fallback = null)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            ".gif"            => "image/gif",
            ".bmp"            => "image/bmp",
            ".svg"            => "image/svg+xml",
            ".avif"           => "image/avif",
            ".heic"           => "image/heic",
            _                 => fallback ?? "application/octet-stream"
        };
    }
}