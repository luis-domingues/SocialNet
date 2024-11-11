using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNet.Context;
using SocialNet.Hubs;
using SocialNet.Models;

namespace SocialNet.Controllers;

public class UsersController : Controller
{
    private readonly SocialNetDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public UsersController(SocialNetDbContext context, IHubContext<NotificationHub> notificationHub)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
        _notificationHub = notificationHub;
    }
    
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user)
    {
        if (ModelState.IsValid)
        {
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }
        return View(user);
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(User user)
    {
        var existingUser = _context.Users
            .FirstOrDefault(u => u.Email == user.Email);

        if (existingUser != null)
        {
            var result = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, user.Password);

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, existingUser.Username),
                    new Claim(ClaimTypes.Email, existingUser.Email),
                    new Claim("UserId", existingUser.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal);

                return RedirectToAction("Index", "Feed");
            }
        }
        
        ModelState.AddModelError(string.Empty, "Credenciais inválidas.");
        return View(user);
    }
    
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user != null)
        {
            var token = Guid.NewGuid().ToString();
            ViewBag.Token = token;
        }

        ViewBag.Message = "Um link de redefinição de senha será enviado para o email registrado, ";
        return View();
    }
    
    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest("Token inválido.");
        
        ViewBag.Token = token;
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string token, string email, string newPassword)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
            return View();
        }

        if (token == null)
        {
            ModelState.AddModelError(string.Empty, "Token inválido.");
            return View();
        }
        
        user.Password = _passwordHasher.HashPassword(user, newPassword);
        await _context.SaveChangesAsync();

        ViewBag.Message = "Senha redefinida com sucesso!";
        return RedirectToAction("Login");
    }
    
    [Authorize]
    public IActionResult Profile()
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

        var user = _context.Users
            .Include(u => u.Posts)
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return NotFound();
        
        return View(user); 
    }

    [Authorize]
    public IActionResult EditProfile()
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return NotFound();

        return View(user);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> EditProfile(User updatedUser)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
        
            await _context.SaveChangesAsync();
            return RedirectToAction("Profile");
        }

        return View(updatedUser);
    }
    
    public IActionResult Following()
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

        var following = _context.Follows
            .Where(f => f.FollowerId == userId)
            .Include(f => f.Followed)
            .Select(f => f.Followed)
            .ToList();

        return View(following);
    }
    
    public IActionResult Followers()
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

        var followers = _context.Follows
            .Where(f => f.FollowedId == userId)
            .Include(f => f.Follower)
            .Select(f => f.Follower)
            .ToList();

        return View(followers);
    }
    
    public IActionResult Search(string query)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

        var users = _context.Users
            .Where(u => u.Username.Contains(query) || u.Email.Contains(query) && u.Id != userId)
            .ToList();
        
        var followingIds = _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId)
            .ToList();
        
        ViewBag.FollowingIds = followingIds;
        return View(users);
    }

    [HttpPost]
    public IActionResult ToggleFollow(int userId)
    {
        var currentUserId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

        if (currentUserId == userId)
            return BadRequest();

        var follow = _context.Follows.FirstOrDefault(f => f.FollowerId == currentUserId && f.FollowedId == userId);

        if (follow == null)
        {
            follow = new Follow
            {
                FollowerId = currentUserId,
                FollowedId = userId
            };
            _context.Follows.Add(follow);
        }
        else
            _context.Follows.Remove(follow);

        _context.SaveChanges();
        return RedirectToAction("Search", new { query = "" });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteAccount()
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        var user = _context.Users
            .Include(u => u.Comments)
            .Include(u => u.Posts)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        var likes = _context.Likes.Where(l => l.UserId == userId).ToList();
        _context.Likes.RemoveRange(likes);

        var comments = _context.Comments.Where(c => c.UserId == userId).ToList();
        _context.Comments.RemoveRange(comments);

        var followerRelations = _context.Follows.Where(f => f.FollowerId == userId || f.FollowedId == userId).ToList();
        _context.Follows.RemoveRange(followerRelations);

        _context.Users.Remove(user);
        _context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> FollowUser(int followedUserId)
    {
        var followerUserId = int.Parse(User.FindFirstValue("UserId"));
        var follower = await _context.Users.FindAsync(followerUserId);

        if (follower == null)
            return BadRequest("Seguidor não encontrado.");

        var follow = new Follow
        {
            FollowerId = followerUserId,
            FollowedId = followedUserId
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        var notification = new Notification
        {
            UserId = followedUserId,
            Type = NotificationType.NewFollower,
            Message = $"@{follower.Username} te seguiu"
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _notificationHub.Clients.User(followedUserId.ToString())
            .SendAsync("ReceiveNotification", notification.Message);
        return Ok();
    }

    [HttpGet]
    public IActionResult ViewProfile(int userId)
    {
        var user = _context.Users
            .Include(u => u.Posts)
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return NotFound();

        return View(user);
    }
}