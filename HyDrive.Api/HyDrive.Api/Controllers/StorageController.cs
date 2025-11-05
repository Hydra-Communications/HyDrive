using HyDrive.Api.DTO;
using HyDrive.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HyDrive.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storageService;

    public StorageController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost("buckets/create")]
    public async Task<ActionResult<string>> CreateBucket([FromBody] CreateBucketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BucketName))
            return BadRequest("Bucket name is required.");

        var testUserId = Guid.NewGuid();

        try
        {
            await _storageService.CreateBucket(request.BucketName, testUserId);
            return Ok($"Bucket '{request.BucketName}' created successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}