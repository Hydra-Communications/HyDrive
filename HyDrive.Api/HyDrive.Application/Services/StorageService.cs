using Domain.Models;
using HyDrive.Api;
using HyDrive.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace HyDrive.Application.Services;

public class StorageService : IStorageService
{
    private readonly AppSettings _appSettings;
    
    public StorageService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }
    
    public async Task AddFileToBucket(Guid bucketId, string bucketName, string fileName, Stream sourceStream)
    {
        // Fail early, fail clearly
        ArgumentNullException.ThrowIfNull(sourceStream);
        if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        
        var bucketDir = bucketId.ToString();
        var pathToStore = Path.Combine(_appSettings.StorageDirectory, bucketDir);
        try
        {
            if (!Directory.Exists(pathToStore))
            {
                Directory.CreateDirectory(pathToStore);
            }

            var finalFilePath = Path.Combine(pathToStore, fileName);
            
            if (File.Exists(finalFilePath))
                throw new IOException($"File '{fileName}' already exists in bucket '{bucketName}'.");
            
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
    
    public async Task<Bucket> GetBucketById(Guid bucketId)
    {
        try
        {
            
        }
        catch (Exception ex)
        {
            
        }
    }
}