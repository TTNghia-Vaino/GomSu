using GomSu.Models;
using GomSu.Services;
using GomSu.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GomSu.Controllers
{
    public class AccountController : Controller
    {
        private readonly GomsuContext _context;
        private readonly EmailService _emailService;

        public AccountController(GomsuContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Account/
        public IActionResult Index()
        {
            // Kiểm tra nếu đã đăng nhập, chuyển hướng về trang chủ
            var maTk = HttpContext.Session.GetString("MaTk");
            if (!string.IsNullOrEmpty(maTk))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ActiveTab = TempData["ActiveTab"] ?? "login";
            return View();
        }

        // GET Login
        [HttpGet]
        public IActionResult Login()
        {
            // Kiểm tra nếu đã đăng nhập, không hiển thị form đăng nhập
            var maTk = HttpContext.Session.GetString("MaTk");
            if (!string.IsNullOrEmpty(maTk))
            {
                return RedirectToAction("Index", "Home");
            }

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
                TempData["ActiveTab"] = "login";
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
            TempData["Message"] = "Đăng xuất thành công!";
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
        public async Task<IActionResult> ForgotPassword(QuenMatKhauModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveTab = "forgot";
                return View("Index", model);
            }

            var user = _context.TaiKhoans.FirstOrDefault(t => t.Email == model.Email);
            if (user == null)
            {
                TempData["Message"] = "Không tồn tại email.";
                TempData["ActiveTab"] = "forgot";
                return RedirectToAction("Index");
            }

            string currentPassword = user.MatKhau;
            string subject = "Thông tin mật khẩu của bạn";
            string body = $"Xin chào {user.HoTen},<br><br>"
                        + "Mật khẩu hiện tại của bạn là: <strong>" + currentPassword + "</strong><br><br>"
                        + "Trân trọng,<br>Gốm Sứ TOTY";

            await _emailService.SendEmailAsync(model.Email, subject, body);

            TempData["Message"] = "Hướng dẫn khôi phục mật khẩu đã được gửi qua email.";
            TempData["ActiveTab"] = "forgot";
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
        public async Task<IActionResult> Register(DangKyModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveTab = "register";
                return View("Index", model);
            }

            var existingUser = _context.TaiKhoans.FirstOrDefault(u => u.TenDangNhap == model.TenDangNhap);
            if (existingUser != null)
            {
                TempData["Message"] = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
                TempData["ActiveTab"] = "register";
                return RedirectToAction("Index");
            }

            var existingEmail = _context.TaiKhoans.FirstOrDefault(u => u.Email == model.Email);
            if (existingEmail != null)
            {
                TempData["Message"] = "Email đã được sử dụng.";
                TempData["ActiveTab"] = "register";
                return RedirectToAction("Index");
            }

            var existingPhone = _context.TaiKhoans.FirstOrDefault(u => u.SoDienThoai == model.SoDienThoai);
            if (existingPhone != null)
            {
                TempData["Message"] = "Số điện thoại đã được sử dụng.";
                TempData["ActiveTab"] = "register";
                return RedirectToAction("Index");
            }
            int newMaTk = (_context.TaiKhoans.Max(tk => (int?)tk.MaTk) ?? 0) + 1;
            // Tạo tài khoản mới
            var newUser = new TaiKhoan
            {
                MaTk = newMaTk,
                HoTen = model.HoTen,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau,
                NgayTao = DateOnly.FromDateTime(DateTime.Now),
                Quyen = 0 // Gán quyền mặc định cho người dùng
            };

            _context.TaiKhoans.Add(newUser);
            _context.SaveChanges();
            string subject = "Chào mừng bạn đến với Gốm Sứ TOTY";
            string body = $"Xin chào {newUser.HoTen},<br><br>" +
                          "Cảm ơn bạn đã đăng ký tài khoản tại <strong>Gốm Sứ TOTY</strong>.<br>" +
                          $"Tên đăng nhập của bạn là: <strong>{newUser.TenDangNhap}</strong><br>" +
                          $"Mật khẩu của bạn là: <strong>{newUser.MatKhau}</strong><br><br>" +
                          "Trân trọng,<br>Gốm Sứ TOTY";

            await _emailService.SendEmailAsync(newUser.Email, subject, body);

            TempData["Message"] = "Đăng ký thành công!";
            TempData["ActiveTab"] = "register";
            return RedirectToAction("Index");
        }
    }
}