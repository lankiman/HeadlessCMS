using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Data.Entities;

[Index(nameof(Slug), IsUnique = true)]
public class Tag : BaseEntity
{
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(150)]
    public string Slug { get; set; }
    
}