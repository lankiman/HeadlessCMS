
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Data.Entities;


[Index(nameof(CreatedAt))]
public abstract class BaseEntity
{
    public Guid Id { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
}