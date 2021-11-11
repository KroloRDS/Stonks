using Microsoft.AspNetCore.Mvc;
using Stonks.Data;
using Stonks.Models;
using System.Diagnostics;

namespace Stonks.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext ctx)
        {
            _logger = logger;
            _ctx = ctx;
        }

        public IActionResult Index()
        {
            _ctx.Log.Add(new Log
            {
                Id = Guid.NewGuid(),
                Message = "Hello",
                Timestamp = DateTime.Now
            });
            _ctx.SaveChanges();

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