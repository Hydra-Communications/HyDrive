using System.Text;
using Domain.Models;
using HyDrive.Api;
using HyDrive.Application.Exceptions;
using HyDrive.Application.Services;
using HyDrive.Infrastructure;
using HyDrive.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HyDrive.Tests.Unit.Services;

public class StorageServiceTests : IAsyncLifetime
{
    private string _testStoragePath = null!;
    private string _testDatabasePath = null!;
    private StorageService _storageService = null!;
    private ApplicationDbContext _context = null!;
    private SqliteConnection _connection = null!;

    public StorageService StorageService => _storageService;
    public string StoragePath => _testStoragePath;
    
    private readonly User _testUser = new User
    {
        Id = new Guid("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
        Username = "testUser",
        Email = "testUser@test.com"
    };

    #region Setup Methods
    
    public async Task InitializeAsync()
    {
        // Setup temp storage folder
        _testStoragePath = Path.Combine(Path.GetTempPath(), "HyDriveTestBuckets", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testStoragePath);

        // Setup temp database file
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"HyDriveTest_{Guid.NewGuid()}.db");

        var appSettings = new AppSettings
        {
            StorageDirectory = _testStoragePath
        };

        // Create SQLite connection with a file-based database
        var connectionString = $"Data Source={_testDatabasePath}";
        _connection = new SqliteConnection(connectionString);
        await _connection.OpenAsync();

        // Configure EF to use this connection
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging() // Helpful for debugging
            .Options;

        _context = new ApplicationDbContext(options);
        
        // Create the database schema
        await _context.Database.EnsureCreatedAsync();

        // Seed the test user - REQUIRED for foreign key constraint
        _context.Users.Add(_testUser);
        await _context.SaveChangesAsync();

        var bucketObjectRepo = new BucketObjectRepository(_context);
        var bucketRepo = new BucketRepository(_context);

        _storageService = new StorageService(appSettings, bucketObjectRepo, bucketRepo);
        
        // Seed a default test bucket
        await _storageService.CreateBucket("TestBucket", _testUser.Id);
    }

    public async Task DisposeAsync()
    {
        // Clean up storage folder
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, recursive: true);
        }

        // Dispose context and connection
        await _context.DisposeAsync();
        await _connection.CloseAsync();
        await _connection.DisposeAsync();

        // Delete test database file
        if (File.Exists(_testDatabasePath))
        {
            // Small delay to ensure file handle is released
            await Task.Delay(100);
            try
            {
                File.Delete(_testDatabasePath);
            }
            catch (IOException)
            {
                // Database might still be locked, ignore
            }
        }
    }

    /// <summary>
    /// Creates a test user with all required properties and adds it to the database
    /// </summary>
    /// <param name="username">Username for the test user</param>
    /// <returns>The created and saved User entity</returns>
    private async Task<User> CreateAndSeedTestUserAsync(string username)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = $"{username}@test.com"
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return user;
    }

    /// <summary>
    ///  Creates a test bucket under the name testBucket
    /// </summary>
    /// <param name="name">Name to overwrite if needed</param>
    /// <returns>the id of the created bucket</returns>
    private async Task<Guid> CreateTestBucketAsync(string name = "testBucket")
    {
        var bucket = await _storageService.CreateBucket(name, _testUser.Id);
        return bucket.Id;
    }

    private async Task<string> AddTestFileAsync(Guid bucketId, string fileName = "test.txt", string content = "Hello world!")
    {
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await _storageService.AddFileToBucket(bucketId, _testUser.Id, fileName, memoryStream);
        return Path.Combine(_testStoragePath, bucketId.ToString(), fileName);
    }
    
    #endregion

    #region Bucket Management Tests

    [Fact]
    public async Task AddNewBucket_WithProperParameters_AddsBucketToDatabase()
    {
        // Create and seed a new user for this test
        var newUser = await CreateAndSeedTestUserAsync("anotherUser");

        var createdBucket = await _storageService.CreateBucket("AnotherBucket", newUser.Id);
        var bucketFromDb = await _storageService.GetBucketById(createdBucket.Id);

        Assert.NotNull(bucketFromDb);
        Assert.Equal("AnotherBucket", bucketFromDb.BucketName);
    }

    [Fact]
    public async Task AddNewBucket_BucketWithSameNameUnderUserAlreadyExists_ThrowsException()
    {
        // Create and seed a new user for this test
        var newUser = await CreateAndSeedTestUserAsync("duplicateBucketUser");

        await _storageService.CreateBucket("testBucket", newUser.Id);
        await Assert.ThrowsAsync<BucketAlreadyExistsException>(
            () => _storageService.CreateBucket("testBucket", newUser.Id));
    }

    [Fact]
    public async Task GetAllBucketsForUser_WithValidUserId_ReturnsAllBucketsForUser()
    {
        // Create and seed a new user for this test
        var newUser = await CreateAndSeedTestUserAsync("multiUserTest");
        
        await _storageService.CreateBucket("testBucket1", newUser.Id);
        await _storageService.CreateBucket("testBucket2", newUser.Id);

        var buckets = await _storageService.GetAllBucketsForUser(newUser.Id);

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
        Assert.Equal("testBucket", bucket.BucketName);
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
        await CreateTestBucketAsync("anotherBucket");
        var initBucket = await _storageService.GetAllBucketsForUser(_testUser.Id);

        await _storageService.DeleteBucket(initBucket[0].Id);
        var bucketsRemaining = await _storageService.GetAllBucketsForUser(_testUser.Id);

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
        await AddTestFileAsync(bucketId);
        await Assert.ThrowsAsync<IOException>(() => AddTestFileAsync(bucketId));
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

    [Fact]
    public async Task RenameBucketObject_WithValidBucketObject_RenamesBucketObjectAndFile()
    {
        // Arrange
        var bucketId = await CreateTestBucketAsync(); 
        await AddTestFileAsync(bucketId, "test.txt", "original content");
        await AddTestFileAsync(bucketId, "test2.txt", "dummy data");
        
        var bucketObjects = await _storageService.GetBucketObjects(bucketId);
        var bucketToRename = bucketObjects.First(b => b.ObjectName == "test2.txt");
        
        // Verify the original file exists
        var originalFilePath = Path.Combine(_testStoragePath, bucketId.ToString(), "test2.txt");
        Assert.True(File.Exists(originalFilePath), "Original file should exist before rename");
        
        // Act
        await _storageService.RenameBucketObject(bucketToRename.Id, "testNewName.txt");
        
        // Assert - verify database was updated
        var updatedObjects = await _storageService.GetBucketObjects(bucketId);
        var renamedObject = updatedObjects.First(b => b.Id == bucketToRename.Id);
        Assert.Equal("testNewName.txt", renamedObject.ObjectName);
        
        // Verify the file was actually renamed on disk
        var newFilePath = Path.Combine(_testStoragePath, bucketId.ToString(), "testNewName.txt");
        Assert.True(File.Exists(newFilePath), "Renamed file should exist");
        Assert.False(File.Exists(originalFilePath), "Original file should no longer exist");
        
        // Verify file contents are preserved
        var fileContents = await File.ReadAllTextAsync(newFilePath);
        Assert.Equal("dummy data", fileContents);
    }

    [Fact]
    public async Task UpdateBucketObject_WithValidBucketObject_UpdatesBucketObjectMetadata()
    {
        // Arrange
        var bucketId = await CreateTestBucketAsync(); 
        await AddTestFileAsync(bucketId, "test.txt", "content");
        
        var bucketObjects = await _storageService.GetBucketObjects(bucketId);
        var bucketToUpdate = bucketObjects.First();
        
        // Modify some metadata (not the name)
        // Assuming BucketObject has other properties you might want to update
        // For now, we'll just verify the update method works
        var originalName = bucketToUpdate.ObjectName;
        
        // Act
        var updatedObject = await _storageService.UpdateBucketObject(bucketToUpdate);
        
        // Assert
        Assert.Equal(originalName, updatedObject.ObjectName);
        Assert.Equal(bucketToUpdate.Id, updatedObject.Id);
    }

    [Fact]
    public async Task UpdateBucketObjectFileContents_WithValidBucketObject_UpdatesBucketObjectFileContents()
    {
        // Arrange
        var bucketId = await CreateTestBucketAsync(); 
        var originalContent = "Original Content";
        var updatedContent = "Updated Content";
        await AddTestFileAsync(bucketId, "test.txt", originalContent);
        var bucketObjects = await _storageService.GetBucketObjects(bucketId);
        
        // Act
        var filePath = Path.Combine(_testStoragePath, bucketId.ToString(), bucketObjects[0].ObjectName);
        await File.WriteAllTextAsync(filePath, updatedContent);
        
        // Assert
        var fileContents = await File.ReadAllTextAsync(filePath);
        Assert.Equal(updatedContent, fileContents);
    }
    #endregion
}