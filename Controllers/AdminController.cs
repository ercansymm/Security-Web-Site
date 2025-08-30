using Microsoft.AspNetCore.Mvc;
using SecurityWebSite.Models;
using Microsoft.EntityFrameworkCore;

namespace SecurityWebSite.Controllers
{
    public class AdminController : Controller
    {
        private readonly SecurityDbContext _context;

        public AdminController(SecurityDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            var adminUser = HttpContext.Session.GetString("AdminUser");
            // Eğer zaten giriş yapmışsa dashboard'a yönlendir
            if (!string.IsNullOrEmpty(adminUser))
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        public async Task<IActionResult> Login(string UserName, string Password)
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Kullanıcı adı ve şifre gereklidir!";
                return View();
            }

            // Kullanıcıyı veritabanında kontrol et
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == UserName && u.UserPassword == Password);

            if (user.IsPassive == false)
            {
                if (user != null)
                {
                    // Session'a kullanıcı bilgisini kaydet
                    HttpContext.Session.SetString("AdminUser", user.UserName);
                    HttpContext.Session.SetString("AdminUserId", user.Ref.ToString());
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                    return View();
                }
            }
            else
            {
                ViewBag.Error = "Kullanıcı Pasif Durumunda!";
                return View();
            }
           
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login");
            }

            // Dashboard için istatistikler
            ViewBag.PersonelCount = await _context.Personels.CountAsync();
            ViewBag.FirmCount = await _context.Firms.CountAsync();
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.AdminUser = HttpContext.Session.GetString("AdminUser");

            return View();
        }

        // GET: Admin/AddPersonel - Personel Yönetim Sayfası
        public IActionResult AddPersonel()
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }



        // AdminController.cs dosyanıza bu metodu ekleyin:

        [HttpPost]
        public async Task<IActionResult> DeletePersonel([FromBody] DeletePersonelRequest request)
        {
            try
            {
                // Personeli bul
                var personel = await _context.Personels.FindAsync(request.PersonelId);

                if (personel == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Personel bulunamadı."
                    });
                }

                // Personel bilgilerini log için sakla (opsiyonel)
                var personelInfo = $"{personel.CardName} {personel.LastName}";

                // Personeli sil
                _context.Personels.Remove(personel);
                await _context.SaveChangesAsync();

                // Başarılı silme
                return Json(new
                {
                    success = true,
                    message = $"{personelInfo} başarıyla silindi.",
                    deletedId = request.PersonelId
                });
            }
            catch (DbUpdateException dbEx)
            {
                // Veritabanı hatası (Foreign key constraint vs.)
                return Json(new
                {
                    success = false,
                    message = "Personel silinemedi. Bu personel başka kayıtlarla ilişkili olabilir."
                });
            }
            catch (Exception ex)
            {
                // Genel hata
                return Json(new
                {
                    success = false,
                    message = "Silme işlemi sırasında bir hata oluştu.",
                    error = ex.Message // Production'da bu satırı kaldırın
                });
            }
        }

        // Request model
        public class DeletePersonelRequest
        {
            public Guid PersonelId { get; set; }
        }
        // GET: Admin/GetAllPersonels - Tüm Personelleri JSON olarak döndür
        [HttpGet]
        public async Task<IActionResult> GetAllPersonels()
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            try
            {
                var personels = await _context.Personels
                    .Select(p => new
                    {
                        p.Ref,
                        p.CardName,
                        p.LastName,
                        p.PhoneNumber,
                        p.City,
                        p.Working,
                        p.Gun,
                        p.Shift,
                        p.YearsOld,
                        p.IsPassive
                    })
                    .OrderBy(p => p.CardName)
                    .ThenBy(p => p.LastName)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = personels,
                    message = $"{personels.Count} personel bulundu."
                });
            }
            catch (Exception ex)
            {
                // Hata loglama yapılabilir
                return Json(new
                {
                    success = false,
                    message = "Personeller yüklenirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // POST: Admin/UpdatePersonelStatus - Personel Aktif/Pasif Durumunu Güncelle
        [HttpPost]
        public async Task<IActionResult> UpdatePersonelStatus([FromBody] UpdateStatusDto model)
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            if (model == null || model.PersonelId == Guid.Empty)
            {
                return Json(new { success = false, message = "Geçersiz istek." });
            }

            try
            {
                // Personeli bul
                var personel = await _context.Personels
                    .FirstOrDefaultAsync(p => p.Ref == model.PersonelId);

                if (personel == null)
                {
                    return Json(new { success = false, message = "Personel bulunamadı." });
                }

                // Eski durum
                var oldStatus = personel.IsPassive;

                // Durumu güncelle
                personel.IsPassive = model.IsPassive;

                // Değişiklikleri kaydet
                _context.Update(personel);
                await _context.SaveChangesAsync();

                // Log kaydı için admin bilgisi
                var adminUser = HttpContext.Session.GetString("AdminUser");
                var statusText = model.IsPassive ? "pasif" : "aktif";

                // Debug için console'a yaz
                Console.WriteLine($"Personel: {personel.CardName} {personel.LastName}");
                Console.WriteLine($"Eski Durum: {(oldStatus == true ? "Pasif" : "Aktif")}");
                Console.WriteLine($"Yeni Durum: {(model.IsPassive ? "Pasif" : "Aktif")}");
                Console.WriteLine($"Admin: {adminUser}");

                return Json(new
                {
                    success = true,
                    message = $"Personel durumu başarıyla {statusText} yapıldı.",
                    data = new
                    {
                        personelId = personel.Ref,
                        isPassive = personel.IsPassive,
                        fullName = $"{personel.CardName} {personel.LastName}",
                        oldStatus = oldStatus,
                        newStatus = model.IsPassive
                    }
                });
            }
            catch (Exception ex)
            {
                // Hata loglama
                Console.WriteLine($"Hata: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = "Durum güncellenirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // GET: Admin/Personeller - Personel Yönetimi için yönlendirme
        public IActionResult Personeller()
        {
            // Giriş kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login");
            }

            return RedirectToAction("AddPersonel");
        }

        // Admin Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Giriş kontrolü için helper method
        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetString("AdminUser") != null;
        }

        // DTO Sınıfı
        public class UpdateStatusDto
        {
            public Guid PersonelId { get; set; }
            public bool IsPassive { get; set; }
        }
    }
}