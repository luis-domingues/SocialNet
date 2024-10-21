using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNet.Models;

public class Follow
{
    public int Id { get; set; }
    
    [ForeignKey("Follower")]
    public int FollowerId { get; set; }
    public User Follower { get; set; }
    
    [ForeignKey("Followed")]
    public int FollowedId { get; set; }
    public User Followed { get; set; }
}