using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GomSu.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace GomSu.Controllers
{
    public class GioHangController : Controller
    {
        private readonly GomsuContext _context;

        public GioHangController(GomsuContext context)
        {
            _context = context;
        }

        // POST: GioHang/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int maSP, int? soLuong)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0) return Unauthorized();

            var sanPham = await _context.SanPhams.FindAsync(maSP);
            if (sanPham == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "SanPham");
            }

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);

            int requestedQuantity = soLuong ?? 1;
            int currentQuantity = gioHang?.SoLuong ?? 0;
            int newTotalQuantity = currentQuantity + requestedQuantity;

            // Calculate true available quantity
            int orderedQuantity = await _context.ChiTietDonHangs
                .Where(c => c.MaSp == maSP && c.MaDonHangNavigation.TrangThai != "Hủy")
                .SumAsync(c => (int?)c.SoLuong) ?? 0;

            int otherUsersCartQuantity = await _context.GioHangs
                .Where(g => g.MaSp == maSP && g.MaTk != maKhachHang)
                .SumAsync(g => (int?)g.SoLuong) ?? 0;

            int availableQuantity = (sanPham.SoLuongTon ?? 0) - (orderedQuantity + otherUsersCartQuantity);

            // Log the values for debugging
            Debug.WriteLine($"AddToCart - MaSP: {maSP}, SoLuongTon: {sanPham.SoLuongTon}, Ordered: {orderedQuantity}, OtherUsers: {otherUsersCartQuantity}, Available: {availableQuantity}, Requested: {newTotalQuantity}");

            if (newTotalQuantity > availableQuantity)
            {
                TempData["Error"] = $"Số lượng vượt quá tồn kho! (Còn lại: {availableQuantity})";
                return RedirectToAction("Index", "SanPham");
            }

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaTk = maKhachHang,
                    MaSp = maSP,
                    SoLuong = requestedQuantity,
                    Gia = sanPham.Gia ?? 0
                };
                _context.GioHangs.Add(gioHang);
            }
            else
            {
                gioHang.SoLuong = newTotalQuantity;
                gioHang.Gia = sanPham.Gia ?? 0;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm vào giỏ hàng thành công!";
            return RedirectToAction("Cart");
        }

        // GET: GioHang/Cart
        public async Task<IActionResult> Cart()
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0) return Unauthorized();

            var gioHang = await _context.GioHangs
                .Include(g => g.MaSpNavigation)
                .Where(g => g.MaTk == maKhachHang)
                .ToListAsync();

            // Calculate available quantities for each product
            var availableQuantities = new Dictionary<int, int>();
            foreach (var item in gioHang)
            {
                if (item.MaSpNavigation == null)
                {
                    TempData["Error"] = $"Không thể tải thông tin sản phẩm (MaSP: {item.MaSp})";
                    return RedirectToAction("Index", "SanPham");
                }

                int orderedQuantity = await _context.ChiTietDonHangs
                    .Where(c => c.MaSp == item.MaSp && c.MaDonHangNavigation.TrangThai != "Hủy")
                    .SumAsync(c => (int?)c.SoLuong) ?? 0;

                int otherUsersCartQuantity = await _context.GioHangs
                    .Where(g => g.MaSp == item.MaSp && g.MaTk != maKhachHang)
                    .SumAsync(g => (int?)g.SoLuong) ?? 0;

                int availableQuantity = (item.MaSpNavigation.SoLuongTon ?? 0) - (orderedQuantity + otherUsersCartQuantity);
                availableQuantities[item.MaSp] = availableQuantity;

                // Log the values for debugging
                Debug.WriteLine($"Cart - MaSP: {item.MaSp}, SoLuongTon: {item.MaSpNavigation.SoLuongTon}, Ordered: {orderedQuantity}, OtherUsers: {otherUsersCartQuantity}, Available: {availableQuantity}");
            }

            ViewBag.AvailableQuantities = availableQuantities;
            return View("~/Views/SanPham/Cart.cshtml", gioHang);
        }

        // POST: GioHang/IncreaseQuantity
        [HttpPost]
        public async Task<IActionResult> IncreaseQuantity(int maSP)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0) return Unauthorized();

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
            if (gioHang != null)
            {
                var sanPham = await _context.SanPhams.FindAsync(maSP);
                if (sanPham != null)
                {
                    int orderedQuantity = await _context.ChiTietDonHangs
                        .Where(c => c.MaSp == maSP && c.MaDonHangNavigation.TrangThai != "Hủy")
                        .SumAsync(c => (int?)c.SoLuong) ?? 0;

                    int otherUsersCartQuantity = await _context.GioHangs
                        .Where(g => g.MaSp == maSP && g.MaTk != maKhachHang)
                        .SumAsync(g => (int?)g.SoLuong) ?? 0;

                    int availableQuantity = (sanPham.SoLuongTon ?? 0) - (orderedQuantity + otherUsersCartQuantity);

                    // Log the values for debugging
                    Debug.WriteLine($"IncreaseQuantity - MaSP: {maSP}, SoLuongTon: {sanPham.SoLuongTon}, Ordered: {orderedQuantity}, OtherUsers: {otherUsersCartQuantity}, Available: {availableQuantity}, NewQuantity: {gioHang.SoLuong + 1}");

                    if (gioHang.SoLuong + 1 <= availableQuantity)
                    {
                        gioHang.SoLuong++;
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Tăng số lượng thành công!";
                    }
                    else
                    {
                        TempData["Error"] = $"Số lượng tồn không đủ! (Còn lại: {availableQuantity})";
                    }
                }
            }
            return RedirectToAction("Cart");
        }

        // POST: GioHang/DecreaseQuantity
        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(int maSP)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0) return Unauthorized();

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
            if (gioHang != null && gioHang.SoLuong > 1)
            {
                gioHang.SoLuong--;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Giảm số lượng thành công!";
            }
            return RedirectToAction("Cart");
        }

        // POST: GioHang/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int maSP)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0) return Unauthorized();

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
            if (gioHang != null)
            {
                _context.GioHangs.Remove(gioHang);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa sản phẩm khỏi giỏ hàng thành công!";
            }
            return RedirectToAction("Cart");
        }

        // POST: GioHang/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int maSP, int soLuong)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0) return Unauthorized();

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
            var sanPham = await _context.SanPhams.FindAsync(maSP);

            if (gioHang != null && sanPham != null)
            {
                int orderedQuantity = await _context.ChiTietDonHangs
                    .Where(c => c.MaSp == maSP && c.MaDonHangNavigation.TrangThai != "Hủy")
                    .SumAsync(c => (int?)c.SoLuong) ?? 0;

                int otherUsersCartQuantity = await _context.GioHangs
                    .Where(g => g.MaSp == maSP && g.MaTk != maKhachHang)
                    .SumAsync(g => g.SoLuong) ?? 0;
                int availableQuantity = (sanPham.SoLuongTon ?? 0) - (orderedQuantity + otherUsersCartQuantity);

                // Log the values for debugging
                Debug.WriteLine($"UpdateQuantity - MaSP: {maSP}, SoLuongTon: {sanPham.SoLuongTon}, Ordered: {orderedQuantity}, OtherUsers: {otherUsersCartQuantity}, Available: {availableQuantity}, Requested: {soLuong}");

                if (soLuong > 0 && soLuong <= availableQuantity)
                {
                    gioHang.SoLuong = soLuong;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật số lượng thành công!";
                }
                else
                {
                    TempData["Error"] = $"Số lượng không hợp lệ hoặc vượt quá tồn kho! (Còn lại: {availableQuantity})";
                }
            }
            else
            {
                TempData["Error"] = "Sản phẩm không tồn tại trong giỏ hàng!";
            }
            return RedirectToAction("Cart");
        }

        private int GetCurrentUserId()
        {
            var maTk = HttpContext.Session.GetString("MaTk");
            return string.IsNullOrEmpty(maTk) ? 0 : Convert.ToInt32(maTk);
        }
    }
}