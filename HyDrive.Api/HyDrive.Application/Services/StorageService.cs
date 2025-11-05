using Domain.Models;
using HyDrive.Api;
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
    
    public async Task AddFileToBucket(Guid bucketId, string bucketName, string fileName, Stream sourceStream)
    {
        // Fail early, fail clearly
        ArgumentNullException.ThrowIfNull(sourceStream);
        if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        
        var bucketDir = bucketId.ToString();
        var pathToStore = Path.Combine(_appSettings.StorageDirectory, bucketDir);
        if (!Directory.Exists(pathToStore))
        {
            try
            {
                Directory.CreateDirectory(pathToStore);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to create directory '{pathToStore}'", ex);
            }
        }

        var finalFilePath = Path.Combine(pathToStore, fileName);

        if (File.Exists(finalFilePath))
        {
            throw new IOException($"File '{fileName}' already exists in bucket '{bucketName}'.");
        }
        
        try
        {
            await using var fileStream = File.Create(finalFilePath);
            await sourceStream.CopyToAsync(fileStream);
        }
        catch (IOException ex)
        {
            throw new IOException($"Failed to add file '{fileName}' to bucket '{bucketName}' ({bucketId}).", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error adding file '{fileName}' to bucket '{bucketName}' ({bucketId}).", ex);
        }
    }

    public async Task<Bucket> CreateBucket(string bucketName, Guid userId)
    {
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
    {
        return await Task.FromResult<Bucket?>(null);
    }
}