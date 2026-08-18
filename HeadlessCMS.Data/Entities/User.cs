using System.ComponentModel.DataAnnotations;
using HeadlessCMS.Data.ValueObjects;

namespace HeadlessCMS.Data.Entities;

public class User : BaseEntity
{
    public Email Email { get; set; }
    
    [MaxLength(1000)]
    public string PasswordHash { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(500)]
    public string? ProfilePicUrl { get; set; }
    
    public bool IsActive { get; set; }
    
    public ICollection<Role> UserRoles { get; set; } = new List<Role>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}