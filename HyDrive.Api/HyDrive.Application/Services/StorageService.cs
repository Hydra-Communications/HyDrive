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
    
    public async Task AddFileToBucket(Guid bucketId, string fileName, Stream sourceStream)
    {
        var pathToStore = Path.Combine(_appSettings.StorageDirectory, "testDir");
        try
        {
            if (!Directory.Exists(pathToStore))
            {
                Directory.CreateDirectory(pathToStore);
            }
            var finalFilePath = pathToStore + fileName;
            var fileStream = File.Create(finalFilePath);
            await sourceStream.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add file {fileName} to bucket {bucketId}", ex);
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