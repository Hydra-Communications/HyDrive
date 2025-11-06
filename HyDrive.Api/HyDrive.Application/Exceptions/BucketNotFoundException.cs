namespace HyDrive.Application.Exceptions;

public class BucketNotFoundException : Exception
{
    public BucketNotFoundException(Guid bucketId, string bucketName)
        : base($"Bucket '{bucketName}' ({bucketId}) does not exist.") { }
}