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
    
    public User TestUser = new User
    {
        Id = new Guid("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
        Username = "testUser"
    };

    #region Setup Methods
    
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
        await _storageService.CreateBucket("TestBucket", TestUser.Id);
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
        var bucket = await _storageService.CreateBucket(name, TestUser.Id);
        return bucket.Id;
    }

    private async Task<string> AddTestFileAsync(Guid bucketId, string fileName = "test.txt", string content = "Hello world!")
    {
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await _storageService.AddFileToBucket(bucketId, Guid.NewGuid(), fileName, memoryStream);
        return Path.Combine(_testStoragePath, bucketId.ToString(), fileName);
    }
    
    #endregion

    #region Bucket Management Tests

    [Fact]
    public async Task AddNewBucket_WithProperParameters_AddsBucketToDatabase()
    {
        var createdBucket = await _storageService.CreateBucket("AnotherBucket", Guid.NewGuid());
        var bucketFromDb = await _storageService.GetBucketById(createdBucket.Id);

        Assert.NotNull(bucketFromDb);
        Assert.Equal("AnotherBucket", bucketFromDb!.BucketName);
    }

    [Fact]
    public async Task AddNewBucket_BucketWithSameNameUnderUserAlreadyExists_ThrowsException()
    {
        var userId = Guid.NewGuid();
        await _storageService.CreateBucket("testBucket", userId);
        await Assert.ThrowsAsync<BucketAlreadyExistsException>(
            () => _storageService.CreateBucket("testBucket", userId));
    }

    [Fact]
    public async Task GetAllBucketsForUser_WithValidUserId_ReturnsAllBucketsForUser()
    {
        var userId = Guid.NewGuid();
        await _storageService.CreateBucket("testBucket1", userId);
        await _storageService.CreateBucket("testBucket2", userId);

        var buckets = await _storageService.GetAllBucketsForUser(userId);

        Assert.Equal(2, buckets.Count);
        Assert.Equal("testBucket1", buckets[0].BucketName);
        Assert.Equal("testBucket2", buckets[1].BucketName);
    }

    [Fact]
    public async Task GetAllBucketsForUser_WithInvalidUserId_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        var buckets = await _storageService.GetAllBucketsForUser(userId);
        Assert.Empty(buckets);
    }

    [Fact]
    public async Task GetBucketById_WithValidBucketId_ReturnsBucket()
    {
        var bucketId = await CreateTestBucketAsync();
        var bucket = await _storageService.GetBucketById(bucketId);

        Assert.NotNull(bucket);
        Assert.Equal("testBucket", bucket!.BucketName);
        Assert.Equal(bucketId, bucket.Id);
    }

    [Fact]
    public async Task GetBucketById_WithInvalidBucketId_ReturnsNull()
    {
        var bucketId = Guid.NewGuid();
        var bucket = await _storageService.GetBucketById(bucketId);
        Assert.Null(bucket);
    }

    [Fact]
    public async Task UpdateBucket_WithValidBucket_UpdatesBucket()
    {
        var bucketId = await CreateTestBucketAsync();
        var initBucket = await _storageService.GetBucketById(bucketId);
        var oldBucketName = initBucket!.BucketName;

        initBucket.BucketName = "UpdatedBucket";
        var updatedBucket = await _storageService.UpdateBucket(initBucket);

        Assert.NotEqual(oldBucketName, updatedBucket.BucketName);
        Assert.Matches("UpdatedBucket", updatedBucket.BucketName);
    }

    [Fact]
    public async Task DeleteBucket_WithValidBucket_DeletesBucket()
    {
        var bucketId = await CreateTestBucketAsync("anotherBucket");
        var initBucket = await _storageService.GetAllBucketsForUser(TestUser.Id);

        await _storageService.DeleteBucket(initBucket[0].Id);
        var bucketsRemaining = await _storageService.GetAllBucketsForUser(TestUser.Id);

        Assert.Single(bucketsRemaining);
    }

    #endregion

    #region BucketObject Management Tests

    [Fact]
    public async Task AddFileToBucket_WithValidStream_AddsFileToBucket()
    {
        var bucketId = await CreateTestBucketAsync();
        var filePath = await AddTestFileAsync(bucketId);
        Assert.True(File.Exists(filePath), "File should exist after being added.");
    }

    [Fact]
    public async Task AddFileToBucket_FileAlreadyExists_ThrowsException()
    {
        var bucketId = await CreateTestBucketAsync();
        var filePath = await AddTestFileAsync(bucketId);
        await Assert.ThrowsAsync<IOException>(() => AddTestFileAsync(bucketId, "test.txt"));
    }

    [Fact]
    public async Task AddFileToBucket_BucketDoesNotExist_ThrowsException()
    {
        var fileName = "test.txt";
        var fileContent = "Hello world!";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        await Assert.ThrowsAsync<BucketNotFoundException>(
            () => _storageService.AddFileToBucket(Guid.NewGuid(), Guid.NewGuid(), fileName, memoryStream));
    }

    [Fact]
    public async Task GetAllBucketObjectsByBucketId_WithValidBucketId_ReturnsAllBucketObjects()
    {
        var bucketId = await CreateTestBucketAsync();
        await AddTestFileAsync(bucketId);
        await AddTestFileAsync(bucketId, "test2.txt");

        var bucketObjects = await _storageService.GetBucketObjects(bucketId);

        Assert.Equal(2, bucketObjects.Count);
    }

    #endregion
}
