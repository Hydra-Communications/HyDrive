namespace HyDrive.Application.Interfaces.Services;

public interface IStorageService
{
    Task AddFileToBucket(Guid bucketId, string bucketName, string fileName, Stream stream);
}