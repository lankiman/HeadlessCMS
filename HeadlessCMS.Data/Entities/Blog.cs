namespace HeadlessCMS.Data.Entities;

public class Blog : BaseEntity
{
    public string Title { get; set; }
    
    public string? FeaturedImageUrl { get; set; }
    
    public string Content { get; set; }
    
    public Guid AuthorId { get; set; }
    
    public User Author { get; set; }
    
}