using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityWebSite.Models;

namespace SecurityWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SecurityDbContext _context; // DbContext eklendi

        public HomeController(ILogger<HomeController> logger, SecurityDbContext context)
        {
            _logger = logger;
            _context = context; // Dependency injection
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Sadece ilk açýklamayý al
                ViewBag.WebDescription = await _context.WebDes
                    .Where(w => !string.IsNullOrEmpty(w.CardDescription))
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                ViewBag.WebDescription = null;
            }

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}