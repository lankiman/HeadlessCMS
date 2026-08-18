using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Data.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Role : BaseEntity
{
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; }
    
}