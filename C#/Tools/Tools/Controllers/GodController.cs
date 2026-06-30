using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Tools.Models;
using Tools.Services;

namespace Tools.Controllers
{
    public class GodController : Controller
    {
        private readonly ILogger<GodController> _logger;

        public GodController(ILogger<GodController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public string GetCheckSum()
        {
            return SkySignature.GenerateCheckSum("51", "1782723646000", "1BFA9166-F683-44EE-8533-A91C697C5D87",
                SkySignature.GenerateNonce(), "URS", "", "d5ba899fba6042428014e82d8a4b2ff3", "4.19.2");
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