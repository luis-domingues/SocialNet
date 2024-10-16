using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNet.Context;
using SocialNet.Models;

namespace SocialNet.Controllers;

public class UsersController : Controller
{
    private readonly SocialNetDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public UsersController(SocialNetDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
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
}