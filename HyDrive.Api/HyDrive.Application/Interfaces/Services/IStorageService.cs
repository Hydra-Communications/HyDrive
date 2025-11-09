using Domain.Models;

namespace HyDrive.Application.Interfaces.Services;

public interface IStorageService
{
    #region Bucket Management
    
    /// <summary>
    /// Creates a new bucket for the given user.
    /// </summary>
    Task<Bucket> CreateBucket(string bucketName, Guid userId);
    
    /// <summary>
    /// Checks whether a bucket exists.
    /// </summary>
    Task<bool> BucketExists(Guid bucketId);
    
    /// <summary>
    /// Gets a bucket by its id.
    /// </summary>
    Task<Bucket?> GetBucketById(Guid bucketId);
    
    /// <summary>
    /// Gets all buckets for a specific user.
    /// </summary>
    Task<List<Bucket>> GetAllBucketsForUser(Guid userId);
    
    /// <summary>
    /// Updates a bucket's metadata.
    /// </summary>
    Task<Bucket> UpdateBucket(Bucket bucket);
    
    /// <summary>
    /// Deletes a bucket.
    /// </summary>
    Task DeleteBucket(Guid bucketId);
    
    #endregion
    
    #region Bucket Object Management
    
    /// <summary>
    /// Adds a new file to the bucket.
    /// </summary>
    Task AddFileToBucket(Guid bucketId, Guid userId, string fileName, Stream stream);
    
    /// <summary>
    /// Gets all bucket objects for a given bucket.
    /// </summary>
    Task<List<BucketObject>> GetBucketObjects(Guid bucketId);
    
    /// <summary>
    /// Updates bucket object metadata in the database.
    /// Does NOT handle file operations.
    /// </summary>
    Task<BucketObject> UpdateBucketObject(BucketObject bucketObject);
    
    /// <summary>
    /// Renames a bucket object and its corresponding file on disk.
    /// </summary>
    Task RenameBucketObject(Guid bucketObjectId, string newName);
    
    /// <summary>
    /// Updates the contents of an existing bucket object file.
    /// </summary>
    Task UpdateBucketObjectFileContents(BucketObject bucketObject, Stream stream);
    
    Task DeleteBucketObject(BucketObject bucketObject);
    
    #endregion
}