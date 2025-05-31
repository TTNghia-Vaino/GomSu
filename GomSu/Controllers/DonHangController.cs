using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GomSu.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace GomSu.Controllers
{
    public class DonHangController : Controller
    {
        private readonly GomsuContext _context;

        public DonHangController(GomsuContext context)
        {
            _context = context;
        }

        // GET: DonHang/Checkout
        public async Task<IActionResult> Checkout()
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Cart", "GioHang");
            }

            var gioHang = await _context.GioHangs
                .Include(g => g.MaSpNavigation)
                .Where(g => g.MaTk == maKhachHang)
                .ToListAsync();

            if (!gioHang.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "SanPham");
            }

            ViewBag.GioHang = gioHang;
            return View();
        }

        // POST: DonHang/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(string diaChi, int? phuongThucThanhToan)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Cart", "GioHang");
            }

            if (string.IsNullOrEmpty(diaChi))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng.";
                return RedirectToAction("Checkout");
            }

            if (!phuongThucThanhToan.HasValue || (phuongThucThanhToan != 1 && phuongThucThanhToan != 2))
            {
                TempData["Error"] = "Phương thức thanh toán không hợp lệ.";
                return RedirectToAction("Checkout");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Kiểm tra tài khoản khách hàng tồn tại
                    var taiKhoan = await _context.TaiKhoans.FindAsync(maKhachHang);
                    if (taiKhoan == null)
                    {
                        TempData["Error"] = "Tài khoản khách hàng không tồn tại.";
                        return RedirectToAction("Cart", "GioHang");
                    }

                    var gioHang = await _context.GioHangs
                        .Include(g => g.MaSpNavigation)
                        .Where(g => g.MaTk == maKhachHang)
                        .ToListAsync();

                    if (!gioHang.Any())
                    {
                        TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                        return RedirectToAction("Index", "SanPham");
                    }

                    // Kiểm tra dữ liệu sản phẩm
                    foreach (var item in gioHang)
                    {
                        if (item.MaSpNavigation == null)
                        {
                            TempData["Error"] = $"Sản phẩm (MaSP: {item.MaSp}) không tồn tại.";
                            return RedirectToAction("Cart", "GioHang");
                        }
                        if (item.SoLuong <= 0 || item.Gia <= 0)
                        {
                            TempData["Error"] = $"Dữ liệu sản phẩm không hợp lệ (MaSP: {item.MaSp}, SoLuong: {item.SoLuong}, Gia: {item.Gia}).";
                            return RedirectToAction("Cart", "GioHang");
                        }
                    }

                    // Tính tổng tiền và ép kiểu về int?
                    decimal tongTienDecimal = gioHang.Sum(item => (item.SoLuong ?? 0) * (item.Gia ?? 0));
                    if (tongTienDecimal <= 0)
                    {
                        TempData["Error"] = "Tổng tiền không hợp lệ.";
                        return RedirectToAction("Cart", "GioHang");
                    }
                    if (tongTienDecimal > int.MaxValue)
                    {
                        TempData["Error"] = "Tổng tiền vượt quá giới hạn cho phép.";
                        return RedirectToAction("Cart", "GioHang");
                    }
                    int? tongTien = (int?)tongTienDecimal;

                    // Tạo đơn hàng
                    var donHang = new DonHang
                    {
                        MaTk = maKhachHang,
                        DiaChi = diaChi,
                        NgayDatHang = DateOnly.FromDateTime(DateTime.Now),
                        TongTien = tongTien,
                        TrangThai = 1, // Chờ duyệt
                        PhuongThucThanhToan = phuongThucThanhToan.Value,
                        TongTienVoucher = 0,
                        MaVoucher = null
                    };

                    _context.DonHangs.Add(donHang);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine($"DonHang Created - MaDonHang: {donHang.MaDonHang}, MaTk: {donHang.MaTk}, TongTien: {donHang.TongTien}");

                    // Thêm chi tiết đơn hàng
                    foreach (var item in gioHang)
                    {
                        var chiTiet = new ChiTietDonHang
                        {
                            MaDonHang = donHang.MaDonHang,
                            MaSp = item.MaSp,
                            SoLuong = item.SoLuong ?? 0,
                            Gia = item.Gia ?? 0
                        };
                        _context.ChiTietDonHangs.Add(chiTiet);
                    }
                    await _context.SaveChangesAsync();
                    Debug.WriteLine($"ChiTietDonHang Created for MaDonHang: {donHang.MaDonHang}");

                    // Xóa giỏ hàng
                    _context.GioHangs.RemoveRange(gioHang);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine($"GioHang Removed for MaTk: {maKhachHang}");

                    await transaction.CommitAsync();
                    Debug.WriteLine($"Checkout Success - MaDonHang: {donHang.MaDonHang}, PhuongThucThanhToan: {phuongThucThanhToan}");

                    // Chuyển hướng dựa trên phương thức thanh toán
                    if (phuongThucThanhToan == 2) // QR
                    {
                        return RedirectToAction("PaymentQR", new { maDonHang = donHang.MaDonHang });
                    }
                    else // COD (phuongThucThanhToan == 1)
                    {
                        ViewBag.MaDonHang = donHang.MaDonHang;
                        ViewBag.DiaChi = diaChi;
                        ViewBag.TongTien = donHang.TongTien;
                        return RedirectToAction("PaymentConfirmation", new { maDonHang = donHang.MaDonHang });
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"Checkout Error: {ex.Message}");
                    Debug.WriteLine($"Inner Exception: {(ex.InnerException != null ? ex.InnerException.Message : "No inner exception")}");
                    Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    TempData["Error"] = $"Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại. Chi tiết: {ex.Message}. Inner: {(ex.InnerException != null ? ex.InnerException.Message : "No inner exception")}";
                    return RedirectToAction("Cart", "GioHang");
                }
            }
        }

        // GET: DonHang/PaymentQR
        public IActionResult PaymentQR(int maDonHang)
        {
            var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == maDonHang);
            if (donHang == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index", "SanPham");
            }

            ViewBag.MaDonHang = maDonHang;
            ViewBag.TongTien = donHang.TongTien;
            ViewBag.DiaChi = donHang.DiaChi;
            ViewBag.QRCodeUrl = "/images/qr_bank_payment.jpg"; // Cập nhật đường dẫn ảnh QR
            ViewBag.QRCodeExists = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "qr_bank_payment.jpg"));
            ViewBag.BankInfo = $"Ngân hàng: Vietcombank\nTên tài khoản: Phan Thanh Thai Tuan\nSố tài khoản: 1040353286\nNội dung: DH#{maDonHang}";
            return View();
        }

        // POST: DonHang/PaymentQR/Confirm
        [HttpPost]
        public IActionResult ConfirmPayment(int maDonHang)
        {
            var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == maDonHang);
            if (donHang == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index", "SanPham");
            }

            ViewBag.MaDonHang = maDonHang;
            ViewBag.DiaChi = donHang.DiaChi;
            ViewBag.TongTien = donHang.TongTien;
            return RedirectToAction("PaymentConfirmation", new { maDonHang = maDonHang });
        }

        // GET: DonHang/PaymentConfirmation
        public IActionResult PaymentConfirmation(int maDonHang)
        {
            var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == maDonHang);
            if (donHang == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index", "SanPham");
            }

            ViewBag.MaDonHang = maDonHang;
            ViewBag.DiaChi = donHang.DiaChi;
            ViewBag.TongTien = donHang.TongTien;
            return View();
        }

        private int GetCurrentUserId()
        {
            var maTk = HttpContext.Session.GetString("MaTk");
            Debug.WriteLine($"GetCurrentUserId - MaTk: {maTk}");

            if (string.IsNullOrEmpty(maTk))
            {
                maTk = "1"; // Default user ID for testing
                HttpContext.Session.SetString("MaTk", maTk);
                Debug.WriteLine($"GetCurrentUserId - Set default MaTk: {maTk}");
            }

            return string.IsNullOrEmpty(maTk) ? 0 : Convert.ToInt32(maTk);
        }
    }
}