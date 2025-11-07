using Domain.Models;

namespace HyDrive.Application.Interfaces.Services;

public interface IStorageService
{
    Task<Bucket> CreateBucket(string bucketName, Guid userId);
    Task AddFileToBucket(Guid bucketId, Guid userId, string fileName, Stream stream);
    Task<Bucket?> GetBucketById(Guid bucketId);
    Task<List<Bucket>> GetAllBucketsForUser(Guid userId);
    Task<List<BucketObject>> GetBucketObjects(Guid bucketId);
}