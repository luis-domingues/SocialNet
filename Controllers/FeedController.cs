﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNet.Context;
using SocialNet.Hubs;
using SocialNet.Models;

namespace SocialNet.Controllers;

[Authorize]
public class FeedController : Controller
{
    private readonly SocialNetDbContext _context;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public FeedController(SocialNetDbContext context, IHubContext<NotificationHub> notificationHub)
    {
        _context = context;
        _notificationHub = notificationHub;
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
    
    public async Task<IActionResult> LikePost(int postId)
{
    var userId = int.Parse(User.FindFirstValue("UserId"));
    var post = await _context.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == postId);

    if (post == null)
        return NotFound();
    

    var like = new Like { UserId = userId, PostId = postId };
    _context.Likes.Add(like);
    await _context.SaveChangesAsync();

    var notification = new Notification
    {
        UserId = post.UserId,
        Type = NotificationType.Like,
        PostId = postId,
        Message = $"@{User.Identity.Name} curtiu seu post!"
    };
    _context.Notifications.Add(notification);
    await _context.SaveChangesAsync();

    await _notificationHub.Clients.User(post.UserId.ToString()).SendAsync("ReceiveNotification", notification.Message);
    return Ok();
}

public async Task<IActionResult> AddComment(int postId, string content, int? parentCommentId)
{
    var userId = int.Parse(User.FindFirstValue("UserId"));
    var comment = new Comment
    {
        PostId = postId,
        UserId = userId,
        Content = content,
        ParentCommentId = parentCommentId
    };

    _context.Comments.Add(comment);
    await _context.SaveChangesAsync();

    var post = await _context.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == postId);
    if (post.UserId != userId)
    {
        var notification = new Notification
        {
            UserId = post.UserId,
            Type = NotificationType.Comment,
            PostId = postId,
            Message = $"@{User.Identity.Name} comentou no seu post!"
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _notificationHub.Clients.User(post.UserId.ToString()).SendAsync("ReceiveNotification", notification.Message);
    }

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