using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SecurityWebSite.Models;

namespace SecurityWebSite.Controllers
{
    public class UsersController : Controller
    {
        private readonly SecurityDbContext _context;

        public UsersController(SecurityDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Ref == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ref,CardName,UserName,UserPassword,PhoneNumber,LastName,IsPassive")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Ref = Guid.NewGuid();
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Ref,CardName,UserName,UserPassword,PhoneNumber,LastName,IsPassive")] User user)
        {
            if (id != user.Ref)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Ref))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Ref == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Ref == id);
        }









        // UsersController'a eklenecek metodlar:

        // GET: Users/AdminEkle - Admin Yönetim Sayfası
        public IActionResult AdminEkle()
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            try
            {
                var admins = await _context.Users
                    .Select(u => new
                    {
                        u.Ref,
                        u.CardName,
                        u.LastName,
                        u.UserName,
                        u.UserPassword,
                        u.PhoneNumber,
                        u.IsPassive
                    })
                    .OrderBy(u => u.CardName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = admins,
                    message = $"{admins.Count} admin bulundu."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Adminler yüklenirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // POST: Users/CreateAdmin - Yeni Admin Ekle
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] Models.User model)
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            if (model == null)
            {
                return Json(new { success = false, message = "Geçersiz veri." });
            }

            try
            {
                // Kullanıcı adı kontrolü
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == model.UserName);

                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Bu kullanıcı adı zaten kullanımda." });
                }

                // Yeni admin oluştur
                var newAdmin = new User
                {
                    Ref = Guid.NewGuid(),
                    CardName = model.CardName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    UserPassword = model.UserPassword, 
                    PhoneNumber = model.PhoneNumber,
                    IsPassive = model.IsPassive
                };

                _context.Users.Add(newAdmin);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Admin başarıyla eklendi.",
                    data = new
                    {
                        adminId = newAdmin.Ref,
                        fullName = $"{newAdmin.CardName} {newAdmin.LastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Admin eklenirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // POST: Users/UpdateAdmin - Admin Güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateAdmin([FromBody] Models.User user)
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            if (user == null || user.Ref == null)
            {
                return Json(new { success = false, message = "Geçersiz veri." });
            }

            try
            {
                var admin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Ref == user.Ref);

                if (admin == null)
                {
                    return Json(new { success = false, message = "Admin bulunamadı." });
                }

                // Kullanıcı adı değiştiyse ve başkası kullanıyorsa kontrol et
                if (admin.UserName != user.UserName)
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserName == user.UserName && u.Ref != user.Ref);

                    if (existingUser != null)
                    {
                        return Json(new { success = false, message = "Bu kullanıcı adı zaten kullanımda." });
                    }
                }

                // Admin bilgilerini güncelle
                admin.CardName = user.CardName;
                admin.LastName = user.LastName;
                admin.UserName = user.UserName;
                admin.PhoneNumber = user.PhoneNumber;
                admin.IsPassive = user.IsPassive;
                admin.UserPassword = user.UserPassword; 

                _context.Update(admin);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Admin başarıyla güncellendi.",
                    data = new
                    {
                        adminId = admin.Ref,
                        fullName = $"{admin.CardName} {admin.LastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Admin güncellenirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdmin([FromBody] DeleteAdminDto model)
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            if (model == null || model.AdminId == Guid.Empty)
            {
                return Json(new { success = false, message = "Geçersiz istek." });
            }

            try
            {
                // Kendini silmeye çalışıyor mu kontrol et
                var currentAdminId = HttpContext.Session.GetString("AdminUserId");
                if (currentAdminId == model.AdminId.ToString())
                {
                    return Json(new { success = false, message = "Kendinizi silemezsiniz." });
                }

                var admin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Ref == model.AdminId);

                if (admin == null)
                {
                    return Json(new { success = false, message = "Admin bulunamadı." });
                }

                // Son admin mi kontrol et
                var adminCount = await _context.Users.CountAsync(u => u.IsPassive != true);
                if (adminCount <= 1 && admin.IsPassive != true)
                {
                    return Json(new { success = false, message = "Sistemde en az bir aktif admin bulunmalıdır." });
                }

                _context.Users.Remove(admin);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Admin başarıyla silindi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Admin silinirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // DTO Sınıfları
        public class AdminDto
        {
            public Guid? Ref { get; set; }
            public string CardName { get; set; }
            public string LastName { get; set; }
            public string UserName { get; set; }
            public string UserPassword { get; set; }
            public string PhoneNumber { get; set; }
            public bool IsPassive { get; set; }
        }

        public class DeleteAdminDto
        {
            public Guid AdminId { get; set; }
        }











    }
}
