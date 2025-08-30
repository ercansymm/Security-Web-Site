using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityWebSite.Models;

public class WebDesController : Controller
{
    private readonly SecurityDbContext _context;

    public WebDesController(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Tek kayıt gönder
        var description = await _context.WebDes.FirstOrDefaultAsync();
        return View(description);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CardDescription")] WebDe webDe)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Mevcut kaydı kontrol et
                var existingRecord = await _context.WebDes.FirstOrDefaultAsync();

                if (existingRecord != null)
                {
                    // Güncelle
                    existingRecord.CardDescription = webDe.CardDescription;
                    _context.Update(existingRecord);
                    TempData["Message"] = "Site açıklaması başarıyla güncellendi.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    // Yeni kayıt oluştur
                    webDe.Ref = Guid.NewGuid();
                    _context.Add(webDe);
                    TempData["Message"] = "Site açıklaması başarıyla oluşturuldu.";
                    TempData["MessageType"] = "success";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Bir hata oluştu: " + ex.Message;
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        // ModelState geçerli değilse hatalarla birlikte geri dön
        TempData["Message"] = "Lütfen formu doğru şekilde doldurun.";
        TempData["MessageType"] = "warning";
        return View("Index", webDe);
    }
}