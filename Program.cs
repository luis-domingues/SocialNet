using Microsoft.EntityFrameworkCore;
using SocialNet.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SocialNetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("CookieAuth").AddCookie("CookieAuth", options =>
{
    options.LoginPath = "/Users/Login";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();