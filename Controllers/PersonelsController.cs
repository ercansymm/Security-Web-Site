using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SecurityWebSite.Models;

namespace SecurityWebSite.Controllers
{
    public class PersonelsController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly SecurityDbContext _context;

        public PersonelsController(SecurityDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Personels
        public async Task<IActionResult> Index()
        {
            return View(await _context.Personels.ToListAsync());
        }

        // GET: Personels/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personels
                .FirstOrDefaultAsync(m => m.Ref == id);
            if (personel == null)
            {
                return NotFound();
            }

            return View(personel);
        }

        // GET: Personels/Create
        public IActionResult Create()
        {
            return View();
        }

        private string GetClientIpAddress()
        {
            string ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            if (ip == "::1")
            {
                ip = "127.0.0.1";
            }

            if (!string.IsNullOrEmpty(ip) && ip.Contains(","))
            {
                ip = ip.Split(',')[0].Trim();
            }

            return ip ?? "unknown";
        }

        // POST: Personels/Create - Vue.js için API endpoint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile Image, string CardName, string LastName,
            string PhoneNumber, string City, int YearsOld, bool Working = false,
            bool Gun = false, bool Shift = false, bool IsAdvice = false)
        {
            try
            {
                string ipAddress = GetClientIpAddress();
                string cacheKey = $"PersonelApplication_{ipAddress}";

                if (_cache.TryGetValue(cacheKey, out List<DateTime> applicationTimes))
                {
                    var recentApplications = applicationTimes
                        .Where(t => t >= DateTime.Now.AddHours(-24))
                        .ToList();

                    if (recentApplications.Count >= 2)
                    {
                        var oldestApplication = recentApplications.OrderBy(t => t).First();
                        var hoursLeft = (int)Math.Ceiling((oldestApplication.AddHours(24) - DateTime.Now).TotalHours);

                        return Json(new
                        {
                            success = false,
                            message = $"24 saat içinde maksimum 2 başvuru yapabilirsiniz. {hoursLeft} saat sonra tekrar deneyebilirsiniz.",
                            rateLimited = true
                        });
                    }

                    recentApplications.Add(DateTime.Now);
                    _cache.Set(cacheKey, recentApplications, TimeSpan.FromHours(24));
                }
                else
                {
                    _cache.Set(cacheKey, new List<DateTime> { DateTime.Now }, TimeSpan.FromHours(24));
                }

                var personel = new Personel
                {
                    Ref = Guid.NewGuid(),
                    CardName = CardName,
                    LastName = LastName,
                    PhoneNumber = PhoneNumber,
                    City = City,
                    YearsOld = YearsOld,
                    Working = Working,
                    Gun = Gun,
                    Shift = Shift,
                    IsAdvice = IsAdvice,
                    IsPassive = true
                };

                if (Image != null && Image.Length > 0)
                {
                    if (Image.Length > 5 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "Dosya boyutu 5MB'dan büyük olamaz." });
                    }

                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(Image.ContentType.ToLower()))
                    {
                        return Json(new { success = false, message = "Sadece JPG, PNG ve GIF dosyaları kabul edilir." });
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await Image.CopyToAsync(memoryStream);
                        personel.image = memoryStream.ToArray();
                    }
                }

                if (string.IsNullOrEmpty(CardName) || string.IsNullOrEmpty(LastName) ||
                    string.IsNullOrEmpty(PhoneNumber) || string.IsNullOrEmpty(City))
                {
                    return Json(new { success = false, message = "Tüm zorunlu alanları doldurunuz." });
                }

                if (YearsOld < 18 || YearsOld > 65)
                {
                    return Json(new { success = false, message = "Yaş 18-65 arasında olmalıdır." });
                }

                _context.Add(personel);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Başvurunuz başarıyla kaydedildi!",
                    personelId = personel.Ref
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.",
                    error = ex.Message
                });
            }
        }

        // POST: Personels/CreateForm - Normal form submit için (fallback)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForm([Bind("Ref,CardName,LastName,PhoneNumber,City,Working,Gun,Shift,YearsOld,IsAdvice,IsPassive,image")] Personel personel)
        {
            if (ModelState.IsValid)
            {
                personel.Ref = Guid.NewGuid();
                _context.Add(personel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("Create", personel);
        }

        // GET: Personels/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personels.FindAsync(id);
            if (personel == null)
            {
                return NotFound();
            }
            return View(personel);
        }

        // POST: Personels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Ref,CardName,LastName,PhoneNumber,City,Working,Gun,Shift,YearsOld,IsAdvice,IsPassive,image")] Personel personel)
        {
            if (id != personel.Ref)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(personel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonelExists(personel.Ref))
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
            return View(personel);
        }

        // GET: Personels/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personels
                .FirstOrDefaultAsync(m => m.Ref == id);
            if (personel == null)
            {
                return NotFound();
            }

            return View(personel);
        }

        // POST: Personels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var personel = await _context.Personels.FindAsync(id);
            if (personel != null)
            {
                _context.Personels.Remove(personel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // API: Personel listesi JSON olarak
        [HttpGet]
        public async Task<IActionResult> GetPersonels()
        {
            var personels = await _context.Personels
                .Select(p => new
                {
                    p.Ref,
                    p.CardName,
                    p.LastName,
                    p.PhoneNumber,
                    p.City,
                    p.YearsOld,
                    p.Working,
                    p.Gun,
                    p.Shift,
                    p.IsAdvice,
                    HasImage = p.image != null
                })
                .ToListAsync();

            return Json(personels);
        }

        // API: Personel fotoğrafını görüntüle
        [HttpGet]
        public async Task<IActionResult> GetPersonelImage(Guid id)
        {
            var personel = await _context.Personels.FindAsync(id);

            if (personel?.image == null)
            {
                return NotFound();
            }

            return File(personel.image, "image/jpeg");
        }

        private bool PersonelExists(Guid id)
        {
            return _context.Personels.Any(e => e.Ref == id);
        }
    }
}