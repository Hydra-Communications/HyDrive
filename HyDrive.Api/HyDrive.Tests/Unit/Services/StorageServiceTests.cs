using System.Text;
using Moq;
using Xunit;

namespace HyDrive.Tests.Unit.Services;

public class StorageServiceTests
{
    [Fact]
    public void AddFileToBucket_WithValidStream_AddsFileToBucket()
    {
        // Arrange
        var storageService = new StorageService();
        var bucketId = Guid.NewGuid();
        var fileName = "test.txt";
        var fileContent = "Hello world!";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        // Act
        storageService.AddFileToBucket(bucketId, fileName, memoryStream);
        var bucket = storageService.GetBucketById(bucketId);

        // Assert
        Assert.Contains(bucket.Files, f => f.FileName == fileName);
    }
}