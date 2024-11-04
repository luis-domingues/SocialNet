using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNet.Models;

public class Comment
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Content { get; set; }

    public DateTime CommentedAt { get; set; } = DateTime.Now;
    
    [ForeignKey("Post")]
    public int PostId { get; set; }
    public Post Post { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }
    
    public int? ParentCommentId { get; set; }
    public Comment ParentComment { get; set; }
    
    public ICollection<Comment> Replies { get; set; }
}