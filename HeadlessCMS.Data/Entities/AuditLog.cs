using System.ComponentModel.DataAnnotations.Schema;
using HeadlessCMS.Common.Enums;

namespace HeadlessCMS.Data.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    
    public User User { get; set; }
    
    public EventType EventType { get; set; }
    
    public Guid EntityId { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string EventData { get; set; }
}