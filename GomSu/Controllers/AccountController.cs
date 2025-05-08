using Microsoft.AspNetCore.Mvc;

namespace GomSu.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
