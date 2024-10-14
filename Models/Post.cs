using System.ComponentModel.DataAnnotations;

namespace SocialNet.Models;

public class Post
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Content { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.Now;
    
    public int UserId { get; set; }
    public User User { get; set; }
}