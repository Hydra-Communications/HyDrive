using System.Text;
using Domain.Models;
using HyDrive.Api;
using HyDrive.Application.Exceptions;
using HyDrive.Application.Services;
using HyDrive.Infrastructure;
using HyDrive.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HyDrive.Tests.Unit.Services;

public class StorageServiceTests : IAsyncLifetime
{
    private string _testStoragePath = null!;
    private StorageService _storageService = null!;
    private ApplicationDbContext _context = null!;

    public StorageService StorageService => _storageService;
    public string StoragePath => _testStoragePath;

    public async Task InitializeAsync()
    {
        // Setup temp storage folder
        _testStoragePath = Path.Combine(Path.GetTempPath(), "HyDriveTestBuckets");
        Directory.CreateDirectory(_testStoragePath);

        var appSettings = new AppSettings
        {
            StorageDirectory = _testStoragePath
        };

        // In-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var bucketObjectRepo = new BucketObjectRepository(_context);
        var bucketRepo = new BucketRepository(_context);

        _storageService = new StorageService(appSettings, bucketObjectRepo, bucketRepo);

        // Seed a default test bucket
        await _storageService.CreateBucket("TestBucket", Guid.NewGuid());
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, recursive: true);
        }

        _context.Dispose();
        return Task.CompletedTask;
    }

    private async Task<Guid> CreateTestBucketAsync(string name = "testBucket")
    {
        var bucket = await _storageService.CreateBucket(name, Guid.NewGuid());
        return bucket.Id;
    }

    private async Task<string> AddTestFileAsync(Guid bucketId, string fileName = "test.txt", string content = "Hello world!")
    {
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await _storageService.AddFileToBucket(bucketId, "testBucket", fileName, memoryStream);
        return Path.Combine(_testStoragePath, bucketId.ToString(), fileName);
    }

    
    // BEGIN TESTS :D
    
    [Fact]
    public async Task AddFileToBucket_WithValidStream_AddsFileToBucket()
    {
        // Arrange
        var bucketId = await CreateTestBucketAsync();

        // Act
        var filePath = await AddTestFileAsync(bucketId);

        // assert
        Assert.True(File.Exists(filePath), "File should exist after being added.");
    }

    [Fact]
    public async Task AddFileToBucket_FileAlreadyExists_ThrowsException()
    {
        // Arrange
        var bucketId = await CreateTestBucketAsync();

        // Act
        var filePath = await AddTestFileAsync(bucketId);

        // Assert
        await Assert.ThrowsAsync<IOException>(
            () => AddTestFileAsync(bucketId, "test.txt"));
    }

    [Fact]
    public async Task AddFileToBucket_BucketDoesNotExist_ThrowsException()
    {
        // Arrange
        var fileName = "test.txt";
        var fileContent = "Hello world!";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        // Act + Assert
        await Assert.ThrowsAsync<BucketNotFoundException>(
            () => _storageService.AddFileToBucket(Guid.NewGuid(), "NonExistentBucket", fileName, memoryStream));
    }

    [Fact]
    public async Task AddNewBucket_WithProperParameters_AddsBucketToDatabase()
    {
        // Arrange
        var createdBucket = await _storageService.CreateBucket("AnotherBucket", Guid.NewGuid());

        // Act
        var bucketFromDb = await _storageService.GetBucketById(createdBucket.Id);

        // Assert
        Assert.NotNull(bucketFromDb);
        Assert.Equal("AnotherBucket", bucketFromDb!.BucketName);
    }

    [Fact]
    public async Task AddNewBucket_BucketWithSameNameUnderUserAlreadyExists_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _storageService.CreateBucket("testBucket", userId);
        
        // Act + Assert
        await Assert.ThrowsAsync<BucketAlreadyExistsException>(
            () => _storageService.CreateBucket("testBucket", userId));
    }
    
    [Fact]
    public async Task GetBucketById_WithValidBucketId_ReturnsBucket()
    {
        // Arrange
        var bucketId = await CreateTestBucketAsync();
        
        // Act
        var bucket = await _storageService.GetBucketById(bucketId);
        
        // Assert
        Assert.NotNull(bucket);
        Assert.Equal("testBucket", bucket!.BucketName);
        Assert.Equal(bucketId, bucket.Id);
    }
    
    [Fact]
    public async Task GetBucketById_WithInvalidBucketId_ReturnsNull()
    {
        // Arrange
        var bucketId = Guid.NewGuid();
        
        // Act
        var bucket = await _storageService.GetBucketById(bucketId);
        
        // Assert
        Assert.Null(bucket);
    }
}
