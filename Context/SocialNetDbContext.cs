using Microsoft.EntityFrameworkCore;
using SocialNet.Models;

namespace SocialNet.Context;

public class SocialNetDbContext : DbContext
{
    public SocialNetDbContext(DbContextOptions<SocialNetDbContext> options) : base(options) {  }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
}