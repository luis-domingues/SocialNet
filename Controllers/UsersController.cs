using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public IActionResult Login(User user)
    {
        var existingUser = _context.Users
            .FirstOrDefault(u => u.Email == user.Email);

        if (existingUser != null)
        {
            var result = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, user.Password);

            if (result == PasswordVerificationResult.Success)
                return RedirectToAction("Index", "Home");
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
}