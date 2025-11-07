using Domain.Models;

namespace HyDrive.Application.Interfaces.Services;

public interface IStorageService
{
    #region Bucket Management
    Task<Bucket> CreateBucket(string bucketName, Guid userId);
    Task<bool> BucketExists(Guid bucketId);
    Task<Bucket?> GetBucketById(Guid bucketId);
    Task<List<Bucket>> GetAllBucketsForUser(Guid userId);
    Task DeleteBucket(Guid bucketId);
    #endregion
    
    #region Bucket Object Management
    Task AddFileToBucket(Guid bucketId, Guid userId, string fileName, Stream stream);
    Task<List<BucketObject>> GetBucketObjects(Guid bucketId);
    Task<BucketObject> UpdateBucketObject(BucketObject bucketObject);
    Task UpdateBucketObjectFileContents(BucketObject bucketObject, Stream stream);
    #endregion
}