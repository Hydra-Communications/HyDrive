using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    
    public void SetUpdated(string updatedByUser)
    {
        UpdatedAt = DateTime.UtcNow;
    }
}