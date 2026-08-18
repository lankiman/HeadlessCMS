using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Data.Entities;

[Index(nameof(Slug), IsUnique = true)]
public class Category : BaseEntity
{
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(150)]
    public string Slug { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
}