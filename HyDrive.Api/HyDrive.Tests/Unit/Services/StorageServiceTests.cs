using System.Text;
using HyDrive.Api;
using HyDrive.Application.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HyDrive.Tests.Unit.Services;

public class StorageServiceTests
{    
    [Fact]
    public void AddFileToBucket_WithValidStream_AddsFileToBucket()
    {
        // Arrange
        var appSettingsMock = new Mock<IOptions<AppSettings>>();
        
        var storageService = new StorageService(appSettingsMock.Object);
        var bucketId = Guid.NewGuid();
        var bucketName = "testBucket";
        var fileName = "test.txt";
        var fileContent = "Hello world!";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        // Act
        storageService.AddFileToBucket(bucketId, bucketName, fileName, memoryStream);
        var bucket = storageService.GetBucketById(bucketId);

        // Assert
        Assert.Contains(bucket.Files, f => f.FileName == fileName);
    }
}