using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNet.Context;
using SocialNet.Models;

namespace SocialNet.Controllers;

[Authorize]
public class FeedController : Controller
{
    private readonly SocialNetDbContext _context;

    public FeedController(SocialNetDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var posts = _context.Posts
            .Include(p => p.User)
            .OrderByDescending(p => p.CreateAt)
            .ToList();

        return View(posts);
    }
        
    public IActionResult CreatePost()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreatePost(Post post)
    {
        if (ModelState.IsValid)
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            post.UserId = userId;

            _context.Posts.Add(post);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        return View(post);
    }
}