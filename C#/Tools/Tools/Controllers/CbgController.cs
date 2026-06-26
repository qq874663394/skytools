using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Tools.Models;

namespace Tools.Controllers
{
    public class CbgController : Controller
    {
        private readonly ILogger<CbgController> _logger;

        public CbgController(ILogger<CbgController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
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