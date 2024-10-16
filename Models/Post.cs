using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNet.Models;

public class Post
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Content { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.Now;
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }
}