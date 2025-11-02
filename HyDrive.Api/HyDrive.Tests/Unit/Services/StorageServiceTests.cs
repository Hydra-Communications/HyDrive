using System.Text;
using HyDrive.Api;
using HyDrive.Application.Services;
using Xunit;

namespace HyDrive.Tests.Unit.Services;

public class StorageServiceTests : IDisposable
{
    private readonly string _testStoragePath;
    private readonly StorageService _storageService;

    public StorageServiceTests()
    {
        _testStoragePath = Path.Combine(Path.GetTempPath(), "HyDriveTestBuckets");

        Directory.CreateDirectory(_testStoragePath);

        var appSettings = new AppSettings
        {
            StorageDirectory = _testStoragePath
        };

        _storageService = new StorageService(appSettings);
    }
    
    public StorageService StorageService => _storageService;
    public string StoragePath => _testStoragePath;

    [Fact]
    public async Task AddFileToBucket_WithValidStream_AddsFileToBucket()
    {
        // Arrange
        var bucketId = Guid.NewGuid();
        var bucketName = "testBucket";
        var fileName = "test.txt";
        var fileContent = "Hello world!";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        // Act
        await _storageService.AddFileToBucket(bucketId, bucketName, fileName, memoryStream);

        // Assert
        var expectedFilePath = Path.Combine(_testStoragePath, bucketId.ToString(), fileName);
        Assert.True(File.Exists(expectedFilePath), "File should exist after being added.");
    }

    [Fact]
    public async Task AddFileToBucket_FileAlreadyExists_ThrowsException()
    {
        // Arrange
        var bucketId = Guid.NewGuid();
        var bucketName = "testBucket";
        var fileName = "test.txt";
        var fileContent = "Hello world!";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        await _storageService.AddFileToBucket(bucketId, bucketName, fileName, memoryStream);
        
        // Act & Assert
        await Assert.ThrowsAsync<IOException>(
            () => _storageService.AddFileToBucket(bucketId, bucketName, fileName, memoryStream));
    }
    
    [Fact]
    public async Task AddFileToBucket_BucketDoesNotExist_ThrowsException()
    {
        // Arrange
        var fileName = "test.txt";
        var fileContent = "Hello world!";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        _storageService.CreateBucket("testBucket", Guid.NewGuid());
        
        // Act & Assert
        await Assert.ThrowsAsync<IOException>(
            () => _storageService.AddFileToBucket(bucketId, bucketName, fileName, memoryStream));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, recursive: true);
        }
    }
}
