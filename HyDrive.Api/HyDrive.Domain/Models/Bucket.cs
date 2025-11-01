namespace Domain.Models;

public class Bucket : BaseEntity
{
    public string BucketName { get; set; } = string.Empty;
    
    public List<BucketObject> BucketObjects { get; set; } = new();
}