namespace Domain.Models;

public class BucketObject : BaseEntity
{
    public Guid BucketId { get; set; }
    public Guid UserId { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    
    public User User { get; set; } = null!;
    public Bucket Bucket { get; set; } = null!;
}