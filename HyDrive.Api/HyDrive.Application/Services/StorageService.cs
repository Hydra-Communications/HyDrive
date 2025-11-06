using Domain.Models;
using HyDrive.Api;
using HyDrive.Application.Exceptions;
using HyDrive.Application.Interfaces.Repositories;
using HyDrive.Application.Interfaces.Services;

namespace HyDrive.Application.Services;

public class StorageService : IStorageService
{
    private readonly AppSettings _appSettings;
    private readonly IBucketObjectRepository _bucketObjects;
    private readonly IBucketRepository _buckets;

    public StorageService(
        AppSettings appSettings,
        IBucketObjectRepository bucketObjectRepository,
        IBucketRepository bucketRepository
    )
    {
        _appSettings = appSettings;
        _bucketObjects = bucketObjectRepository;
        _buckets = bucketRepository;
    }

    private async Task<bool> BucketExists(Guid bucketId)
        => await _buckets.GetByIdAsync(bucketId) != null;

    public async Task<List<Bucket>> GetAllBucketsForUserAsync(Guid userId)
    {
        return await _buckets.GetAllByUserIdAsync(userId);
    }

    public async Task AddFileToBucket(Guid bucketId, string bucketName, string fileName, Stream sourceStream)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);
        if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        
        if (!await BucketExists(bucketId))
            throw new BucketNotFoundException(bucketId, bucketName);
        
        var finalFilePath = GetFilePath(bucketId, fileName);
        
        if (File.Exists(finalFilePath))
            throw new IOException($"File '{fileName}' already exists in bucket '{bucketName}'.");
        
        Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath)!);
        
        await using var fileStream = File.Create(finalFilePath);
        await sourceStream.CopyToAsync(fileStream);
    }

    public async Task<Bucket> CreateBucket(string bucketName, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new ArgumentNullException(nameof(bucketName));

        var usersBuckets = await GetAllBucketsForUserAsync(userId);
        if (usersBuckets.Any(b => b.BucketName == bucketName))
        {
            throw new BucketAlreadyExistsException(bucketName);
        }

        var newBucket = new Bucket
        {
            BucketName = bucketName,
            UserId = userId
        };

        await _buckets.AddAsync(newBucket);
        await _buckets.SaveAsync();

        return newBucket;
    }

    public async Task<Bucket?> GetBucketById(Guid bucketId)
        => await _buckets.GetByIdAsync(bucketId);
    
    private string GetFilePath(Guid bucketId, string fileName)
        => Path.Join(_appSettings.StorageDirectory, bucketId.ToString(), fileName);
}
