using Domain.Models;

namespace HyDrive.Application.Interfaces.Services;

public interface IStorageService
{
    Task<Bucket> CreateBucket(string bucketName, Guid userId);
    Task AddFileToBucket(Guid bucketId, string bucketName, string fileName, Stream stream);
}