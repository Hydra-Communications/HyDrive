using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class BucketObject : BaseEntity
{
    public Guid BucketId { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    
    public Bucket Bucket { get; set; } = null!;
}