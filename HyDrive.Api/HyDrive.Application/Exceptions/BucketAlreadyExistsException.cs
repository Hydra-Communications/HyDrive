namespace HyDrive.Application.Exceptions;

public class BucketAlreadyExistsException : Exception
{
    public BucketAlreadyExistsException(string bucketName)
        : base($"Bucket under the name: '{bucketName}' already exists under this account.") { }
}