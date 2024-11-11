namespace SocialNet.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; } 
    public User User { get; set; }
    
    public NotificationType Type { get; set; }
    public string Message { get; set; }
    
    public int? PostId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public bool IsRead { get; set; } = false;
}