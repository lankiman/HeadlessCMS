
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Data.Entities;

[Index(nameof(Id))]
[Index(nameof(CreatedAt))]
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
}