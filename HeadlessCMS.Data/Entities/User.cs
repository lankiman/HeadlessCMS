using HeadlessCMS.Data.ValueObjects;

namespace HeadlessCMS.Data.Entities;

public class User : BaseEntity
{
    public string? ProfilePicUrl { get; set; }
    
    public string PasswordHash { get; set; }
    
    public Guid RoleId { get; set; }
    
    public Role Role { get; set; }
    
    public string Name { get; set; }
    
    public Email Email { get; set; }
}