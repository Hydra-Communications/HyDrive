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

    #region Bucket Management
    /// <summary>
    /// Checks whether a bucket under a given id exists or not
    /// </summary>
    /// <param name="bucketId">The bucketId for which to search for</param>
    /// <returns>A boolean; true if bucket exists, false if not</returns>
    public async Task<bool> BucketExists(Guid bucketId)
        => await _buckets.GetByIdAsync(bucketId) != null;
    
    /// <summary>
    /// Gets all buckets linked to a given user
    /// </summary>
    /// <param name="userId">The userId for which the method searches for</param>
    /// <returns>A list of buckets</returns>
    public async Task<List<Bucket>> GetAllBucketsForUser(Guid userId)
    {
        return await _buckets.GetAllByUserIdAsync(userId);
    }

    /// <summary>
    /// Creates a new bucket for the given user.
    /// </summary>
    /// <param name="bucketName">The new bucket's name</param>
    /// <param name="userId">The userId to which the bucket is linked</param>
    /// <returns>A completed task with the created bucket object</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="BucketAlreadyExistsException"></exception>
    public async Task<Bucket> CreateBucket(string bucketName, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new ArgumentNullException(nameof(bucketName));

        var usersBuckets = await GetAllBucketsForUser(userId);
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

    /// <summary>
    /// Gets a bucket by its id
    /// </summary>
    /// <param name="bucketId">the bucket id for which to search with</param>
    /// <returns>A bucket</returns>
    public async Task<Bucket?> GetBucketById(Guid bucketId)
        => await _buckets.GetByIdAsync(bucketId);

    /// <summary>
    /// Updates a bucket with new values
    /// </summary>
    /// <param name="bucket">The bucket object for which to be updated (find using the getBucketById method)</param>
    /// <returns>The updated bucket's new values</returns>
    public async Task<Bucket> UpdateBucket(Bucket bucket)
    {
        var update = await _buckets.UpdateAsync(bucket);
        await _buckets.SaveAsync();

        return update;
    }

    /// <summary>
    /// Deletes a bucket.
    /// Decision was made to silently deal with the problem
    /// if there is no bucket to delete to reduce complexity/overlogging
    /// </summary>
    /// <param name="bucketId">The bucket to delete</param>
    public async Task DeleteBucket(Guid bucketId)
    {
        await _buckets.DeleteByIdAsync(bucketId);
        await _buckets.SaveAsync();
    }
    #endregion Bucket Management

    #region Bucket Object Management
    /// <summary>
    /// Adds a new file to the bucket, creating a new file and directory if need be.
    /// </summary>
    /// <param name="bucketId">The bucket to which the new bucketObject will be assigned</param>
    /// <param name="userId">The userId to which the new bucketObject will be assigned</param>
    /// <param name="fileName">The file's name</param>
    /// <param name="sourceStream">The input stream for the file's data</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="BucketNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public async Task AddFileToBucket(Guid bucketId, Guid userId, string fileName, Stream sourceStream)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        var parentBucket = await GetBucketById(bucketId);
        var parentBucketName = parentBucket?.BucketName ?? "Unknown";
        
        if (!await BucketExists(bucketId))
            throw new BucketNotFoundException(bucketId, parentBucket?.BucketName ?? "Unknown");
        
        var finalFilePath = GetFilePath(bucketId, fileName);
        
        if (File.Exists(finalFilePath))
            throw new IOException($"File '{fileName}' already exists in bucket '{parentBucketName}'.");
        
        Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath)!);

        var newBucketObject = new BucketObject
        {
            BucketId = bucketId,
            UserId = userId,
            ObjectName = fileName
        };
        await _bucketObjects.AddAsync(newBucketObject);
        
        await using var fileStream = File.Create(finalFilePath);
        await sourceStream.CopyToAsync(fileStream);
        await _buckets.SaveAsync();
        await _bucketObjects.SaveAsync();
    }
    
    /// <summary>
    /// Gets all bucket objects for a given bucket
    /// </summary>
    /// <param name="bucketId">The parent bucket's id</param>
    /// <returns>A list of bucket objects</returns>
    public async Task<List<BucketObject>> GetBucketObjects(Guid bucketId)
    {
        return await _bucketObjects.GetAllByBucketId(bucketId);
    }

    /// <summary>
    /// Updates the database side & or the namee of the bucket object.
    /// </summary>
    /// <param name="bucketObject">The bucket object to be updated</param>
    /// <returns>The updated bucket object</returns>
    public async Task<BucketObject> UpdateBucketObject(BucketObject bucketObject)
    {
        ArgumentNullException.ThrowIfNull(bucketObject);

        var filePath = GetFilePath(bucketObject.BucketId, bucketObject.ObjectName);
        var currentBucketObject = await _bucketObjects.GetByIdAsync(bucketObject.Id)
                                  ?? throw new BucketObjectNotFoundException(bucketObject.Id, bucketObject.ObjectName);

        var oldFilePath = GetFilePath(currentBucketObject.BucketId, currentBucketObject.ObjectName);

        // Ensure old file exists
        if (!File.Exists(oldFilePath))
            throw new FileNotFoundException("Cannot update file because it does not exist.", oldFilePath);

        // Handle renaming (if the object name changed)
        if (!string.Equals(bucketObject.ObjectName, currentBucketObject.ObjectName, StringComparison.Ordinal))
        {
            var newFilePath = GetFilePath(bucketObject.BucketId, bucketObject.ObjectName);

            // Prevent overwriting an existing file with the new name
            if (File.Exists(newFilePath))
                throw new IOException($"A file with the name '{bucketObject.ObjectName}' already exists in this bucket.");

            // Ensure the new directory exists (in case bucket structure changed)
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath)!);

            // Move (rename) the file
            File.Move(oldFilePath, newFilePath);
        }

        // Update metadata in the database
        var updatedObject = await _bucketObjects.UpdateAsync(bucketObject);
        await _bucketObjects.SaveAsync();

        return updatedObject;
    }


    public async Task UpdateBucketObjectFileContents(BucketObject bucketObject, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(bucketObject);
        ArgumentNullException.ThrowIfNull(stream);
        
        var filePath = GetFilePath(bucketObject.BucketId, bucketObject.ObjectName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Cannot update file because it does not exist.", filePath);
        
        await using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream);
    }

    #endregion Bucket Object Management
    
    #region Helper methods
    
    /// <summary>
    /// Creates the path to a file from its name and bucketId.
    /// </summary>
    /// <param name="bucketId">The file's buceketid</param>
    /// <param name="fileName">The file's name and extension</param>
    /// <returns></returns>
    private string GetFilePath(Guid bucketId, string fileName)
        => Path.Join(_appSettings.StorageDirectory, bucketId.ToString(), fileName);
    
    #endregion Helper methods
}
