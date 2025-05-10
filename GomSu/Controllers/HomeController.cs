using System.Diagnostics;
using GomSu.Models;
using GomSu.Services;
using Microsoft.AspNetCore.Mvc;

namespace GomSu.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmailService _emailService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, EmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendMail()
        {
            await _emailService.SendEmailAsync("Email@gmail.com", "Test", "Hello from SMTP!");
            TempData["Message"] = "Email sent!";
            return RedirectToAction("Index");
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
