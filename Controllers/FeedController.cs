﻿using Microsoft.AspNetCore.Authorization;
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
            .Include(p => p.Likes)
            .Include(p => p.Comments).ThenInclude(c => c.User)
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
    public IActionResult AddComment(int postId, string content)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var comment = new Comment
        {
            Content = content,
            PostId = postId,
            UserId = userId
        };

        _context.Comments.Add(comment);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}