using System.ComponentModel.DataAnnotations;
using HeadlessCMS.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Data.Entities;

[Index(nameof(Slug), IsUnique = true)]
public class Post : BaseEntity
{
    [MaxLength(200)]
    public string Title { get; set; }
    
    [MaxLength(300)]
    public string Slug { get; set; }
    
    public string Content { get; set; }
    
    public string CategoryId { get; set; }
    
    public Category Category { get; set; }
    
    public Guid AuthorId { get; set; }
    public User Author { get; set; }
    
    [MaxLength(500)]
    public string? FeaturedImageUrl { get; set; }
    
    public PostStatus Status { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
    
    public string? ReviewComment { get; set; }
    
    public string? ReviewerId { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}