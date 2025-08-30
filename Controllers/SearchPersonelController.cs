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
    public class SearchPersonelController : Controller
    {
        private readonly SecurityDbContext _context;

        public SearchPersonelController(SecurityDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Personels.ToListAsync());
        }

        // POST: SearchPersonel/Search - ARAMA METODU
        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchFilterDto searchFilters)
        {
            try
            {
                // Temel sorguyu başlat - sadece aktif personeller
                var query = _context.Personels.Where(p => p.IsPassive != true);

                // Şehir filtresi
                if (!string.IsNullOrEmpty(searchFilters.City))
                {
                    query = query.Where(p => p.City == searchFilters.City);
                }

                // Yaş aralığı filtresi
                if (searchFilters.MinAge.HasValue)
                {
                    query = query.Where(p => p.YearsOld.HasValue && p.YearsOld.Value >= searchFilters.MinAge.Value);
                }

                if (searchFilters.MaxAge.HasValue)
                {
                    query = query.Where(p => p.YearsOld.HasValue && p.YearsOld.Value <= searchFilters.MaxAge.Value);
                }

                // Çalışma durumu filtresi
                if (searchFilters.IncludeWorking && !searchFilters.IncludeAvailable)
                {
                    query = query.Where(p => p.Working == true);
                }
                else if (!searchFilters.IncludeWorking && searchFilters.IncludeAvailable)
                {
                    query = query.Where(p => p.Working == false);
                }

                // Vardiya filtresi
                if (searchFilters.ShiftAvailable.HasValue)
                {
                    if (searchFilters.ShiftAvailable.Value)
                    {
                        query = query.Where(p => p.Shift == true);
                    }
                    else if (searchFilters.ShiftSelected)
                    {
                        query = query.Where(p => p.Shift == false);
                    }
                }

                // Güvenlik türü filtresi
                if (!string.IsNullOrEmpty(searchFilters.GunType))
                {
                    switch (searchFilters.GunType.ToLower())
                    {
                        case "armed":
                            query = query.Where(p => p.Gun == true);
                            break;
                        case "unarmed":
                            query = query.Where(p => p.Gun == false);
                            break;
                        case "both":
                            // Hepsi - filtre uygulamaya gerek yok
                            break;
                    }
                }

                // Pozisyon türü filtresi (IsAdvice)
                if (!string.IsNullOrEmpty(searchFilters.PositionType))
                {
                    switch (searchFilters.PositionType.ToLower())
                    {
                        case "advice":
                            query = query.Where(p => p.IsAdvice == true);
                            break;
                        case "security":
                            query = query.Where(p => p.IsAdvice == false);
                            break;
                        case "all":
                            // Hepsi - filtre uygulamaya gerek yok
                            break;
                    }
                }

                var results = await query
                    .Select(p => new PersonelSearchResultDto
                    {
                        Id = p.Ref,
                        CardName = p.CardName,
                        LastName = p.LastName,
                        PhoneNumber = p.PhoneNumber,
                        City = p.City,
                        Working = p.Working ?? false,
                        Gun = p.Gun ?? false,
                        Shift = p.Shift ?? false,
                        YearsOld = p.YearsOld ?? 0,
                        IsAdvice = p.IsAdvice ?? false,
                        ImagePath = p.image != null && p.image.Length > 0
                            ? "data:image/jpeg;base64," + Convert.ToBase64String(p.image)
                            : null
                    })
                    .OrderBy(p => p.CardName)
                    .ThenBy(p => p.LastName)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = results,
                    message = $"{results.Count} personel bulundu."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    data = new List<PersonelSearchResultDto>(),
                    message = "Arama sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin."
                });
            }
        }

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

        // GET: SearchPersonel/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SearchPersonel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ref,CardName,LastName,PhoneNumber,City,Working,Gun,Shift,YearsOld,IsAdvice,IsPassive,image")] Personel personel)
        {
            if (ModelState.IsValid)
            {
                personel.Ref = Guid.NewGuid();
                _context.Add(personel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(personel);
        }

        // GET: SearchPersonel/Edit/5
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

        // POST: SearchPersonel/Edit/5
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

        // GET: SearchPersonel/Delete/5
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

        // POST: SearchPersonel/Delete/5
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

        private bool PersonelExists(Guid id)
        {
            return _context.Personels.Any(e => e.Ref == id);
        }
    }

    // DTO SINIFLAR
    public class SearchFilterDto
    {
        public string City { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public bool IncludeWorking { get; set; }
        public bool IncludeAvailable { get; set; }
        public bool? ShiftAvailable { get; set; }
        public bool ShiftSelected { get; set; }
        public string GunType { get; set; }
        public string PositionType { get; set; } 
    }

    public class PersonelSearchResultDto
    {
        public Guid Id { get; set; }
        public string CardName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public bool Working { get; set; }
        public bool Gun { get; set; }
        public bool Shift { get; set; }
        public int YearsOld { get; set; }
        public bool IsAdvice { get; set; }
        public string ImagePath { get; set; }
    }
}