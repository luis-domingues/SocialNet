using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNet.Models;

public class Like
{
    public int Id { get; set; }

    [ForeignKey("Post")]
    public int PostId { get; set; }
    public Post Post { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }
}