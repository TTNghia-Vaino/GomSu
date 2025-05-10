using GomSu.Models;
using GomSu.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GomSu.Controllers
{
    public class AccountController : Controller
    {
        private readonly GomsuContext _context;

        public AccountController(GomsuContext context)
        {
            _context = context;
        }

        // GET: /Account/
        public IActionResult Index()
        {
            // Kiểm tra tab nào được chọn sau khi redirect (từ TempData)
            ViewBag.ActiveTab = TempData["ActiveTab"] ?? "login";
            return View();
        }

        // GET Login
        [HttpGet]
        public IActionResult Login()
        {
            return PartialView("_LoginPartial", new DangNhapModel());
        }

        // POST Login
        [HttpPost]
        public IActionResult Login(DangNhapModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveTab = "login";
                return View("Index", model);
            }

            var user = _context.TaiKhoans.FirstOrDefault(u =>
                u.TenDangNhap == model.TenDangNhap && u.MatKhau == model.MatKhau);

            if (user == null)
            {
                TempData["Error"] = "Sai tên đăng nhập hoặc mật khẩu.";
                TempData["ActiveTab"] = "login"; // Giữ lại tab
                return RedirectToAction("Index");
            }
            // Lưu thông tin vào HttpContext.Session
            HttpContext.Session.SetString("HoTen", user.HoTen);
            HttpContext.Session.SetString("Quyen", user.Quyen.ToString()); 
            HttpContext.Session.SetString("MaTk", user.MaTk.ToString()); 
            TempData["Message"] = "Đăng nhập thành công!";

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        // GET Forgot Password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return PartialView("_ForgotPasswordPartial", new QuenMatKhauModel());
        }

        // POST Forgot Password
        [HttpPost]
        public IActionResult ForgotPassword(QuenMatKhauModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveTab = "forgot";
                return View("Index", model);
            }

            // TODO: xử lý quên mật khẩu
            TempData["Message"] = "Hướng dẫn khôi phục mật khẩu đã được gửi qua email.";
            TempData["ActiveTab"] = "forgot"; // Giữ lại tab
            return RedirectToAction("Index");
        }

        // GET Register
        [HttpGet]
        public IActionResult Register()
        {
            return PartialView("_RegisterPartial", new DangKyModel());
        }

        // POST Register
        [HttpPost]
        public IActionResult Register(DangKyModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveTab = "register";
                return View("Index", model);
            }

            // TODO: xử lý đăng ký người dùng mới
            TempData["Message"] = "Đăng ký thành công!";
            TempData["ActiveTab"] = "register"; // Giữ lại tab
            return RedirectToAction("Index");
        }
    }
}
