using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNet.Models;

public class User
{
    public int Id { get; set; }
        
    [Required]
    [StringLength(50)]
    public string Username { get; set; }
        
    [Required]
    [StringLength(256)]
    public string Email { get; set; }
        
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Password { get; set; }
    public string Bio { get; set; }
    
    public ICollection<Post> Posts { get; set; }
    public ICollection<Follow> Followers { get; set; }
    public ICollection<Follow> Following { get; set; }
    public ICollection<Comment> Comments { get; set; }
}

