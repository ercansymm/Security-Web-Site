using Microsoft.EntityFrameworkCore;
using SecurityWebSite.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<SecurityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Admin route'larý
app.MapControllerRoute(
    name: "adminLogin",
    pattern: "yonetim",
    defaults: new { controller = "Admin", action = "Login" });

app.MapControllerRoute(
    name: "adminDashboard",
    pattern: "yonetim/panel",
    defaults: new { controller = "Admin", action = "Dashboard" });

app.MapControllerRoute(
    name: "adminActions",
    pattern: "yonetim/{action}/{id?}",
    defaults: new { controller = "Admin", action = "Login" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();