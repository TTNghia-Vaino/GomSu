using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GomSu.Models;
using System.Threading.Tasks;
using System.Linq;

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
            if (sanPham == null || sanPham.SoLuongTon < (soLuong ?? 1))
            {
                TempData["Error"] = "Sản phẩm không tồn tại hoặc số lượng không đủ.";
                return RedirectToAction("Index", "SanPham");
            }

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaTk = maKhachHang,
                    MaSp = maSP,
                    SoLuong = soLuong ?? 1,
                    Gia = (int)sanPham.Gia
                };
                _context.GioHangs.Add(gioHang);
            }
            else
            {
                gioHang.SoLuong += (int)(soLuong ?? 1);
                gioHang.Gia = (int)sanPham.Gia;
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
                if (sanPham != null && sanPham.SoLuongTon > gioHang.SoLuong)
                {
                    gioHang.SoLuong++;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tăng số lượng thành công!";
                }
                else
                {
                    TempData["Error"] = "Số lượng tồn không đủ!";
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

        private int GetCurrentUserId()
        {
            var maTk = HttpContext.Session.GetString("MaTk");
            return string.IsNullOrEmpty(maTk) ? 0 : Convert.ToInt32(maTk);
        }
    }
}