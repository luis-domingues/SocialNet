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
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

        var followedUserIds = _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId)
            .ToList();

        var posts = _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .Where(p => followedUserIds.Contains(p.UserId) || p.UserId == userId)
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

    [HttpPost]
    public IActionResult ToggleLike(int postId)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var like = _context.Likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);

        if (like == null)
        {
            like = new Like
            {
                PostId = postId,
                UserId = userId
            };
            _context.Likes.Add(like);
        }
        else
            _context.Likes.Remove(like);

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int postId, string content, int? parentCommentId)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            Content = content,
            ParentCommentId = parentCommentId,
            CommentedAt = DateTime.Now
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new { postId });
    }


    public IActionResult EditPost(int postId)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var post = _context.Posts.FirstOrDefault(p => p.Id == postId && p.UserId == userId);

        if (post == null)
            return NotFound();
        

        return View(post);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditPost(Post updatedPost)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var post = _context.Posts.FirstOrDefault(p => p.Id == updatedPost.Id && p.UserId == userId);

        if (post == null)
            return NotFound();
        

        if (ModelState.IsValid)
        {
            post.Content = updatedPost.Content;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(updatedPost);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int postId)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var post = _context.Posts.FirstOrDefault(p => p.Id == postId && p.UserId == userId);

        if (post == null)
            return NotFound();
        

        _context.Posts.Remove(post);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}