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

// ==================== CUSTOM ROUTES ====================

// Ana Sayfa
app.MapControllerRoute(
    name: "anasayfa",
    pattern: "anasayfa",
    defaults: new { controller = "Home", action = "Index" });

// Bizimle Çalýþ
app.MapControllerRoute(
    name: "bizimle-calis",
    pattern: "bizimle-calis",
    defaults: new { controller = "Personels", action = "Index" });

// Personel Ara
app.MapControllerRoute(
    name: "personel-ara",
    pattern: "personel-ara",
    defaults: new { controller = "SearchPersonel", action = "Index" });



// ==================== ADMIN ROUTES ====================

// Admin Login (yonetim yazýnca login sayfasýna gitsin)
app.MapControllerRoute(
    name: "adminLogin",
    pattern: "yonetim",
    defaults: new { controller = "Admin", action = "Login" });

// Admin Dashboard
app.MapControllerRoute(
    name: "adminDashboard",
    pattern: "yonetim/panel",
    defaults: new { controller = "Admin", action = "Dashboard" });

// Admin Personel Listesi
app.MapControllerRoute(
    name: "adminPersonelList",
    pattern: "yonetim/personel-listesi",
    defaults: new { controller = "Admin", action = "AddPersonel" });

// Admin Personel Ekle (ID parametresi ile)
app.MapControllerRoute(
    name: "adminPersonelActions",
    pattern: "yonetim/personel-listesi/{id?}",
    defaults: new { controller = "Admin", action = "AddPersonel" });

// Adminler Listesi
app.MapControllerRoute(
    name: "adminUsers",
    pattern: "yonetim/adminler",
    defaults: new { controller = "Users", action = "Index" });

// Site Açýklamasý (Admin içinde)
app.MapControllerRoute(
    name: "adminSiteDescription",
    pattern: "yonetim/site-aciklamasi",
    defaults: new { controller = "WebDes", action = "Index" });

// Genel Admin Actions (diðer admin iþlemleri için)
app.MapControllerRoute(
    name: "adminActions",
    pattern: "yonetim/{action}/{id?}",
    defaults: new { controller = "Admin", action = "Login" });

// ==================== DEFAULT ROUTE ====================
// En sonda olmalý, diðer route'lar eþleþmezse bu çalýþýr
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "adminLogout",
    pattern: "yonetim/cikis",
    defaults: new { controller = "Admin", action = "Logout" });

app.Run();