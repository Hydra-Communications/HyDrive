namespace HyDrive.Application.Exceptions;

public class BucketObjectNotFoundException : Exception
{
    public BucketObjectNotFoundException(Guid bucketObjectId, string bucketObjectName)
        : base($"Bucket object '{bucketObjectName}' ({bucketObjectId}) does not exist.") { }
}