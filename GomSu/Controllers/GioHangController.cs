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
            if (maKhachHang == 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Index", "Account");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var sanPham = await _context.SanPhams.FindAsync(maSP);
                    if (sanPham == null)
                    {
                        TempData["Error"] = "Sản phẩm không tồn tại.";
                        return RedirectToAction("Index", "SanPham");
                    }

                    var gioHang = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);

                    int requestedQuantity = soLuong ?? 1;

                    // Kiểm tra số lượng tồn kho khả dụng
                    if (requestedQuantity > (sanPham.SoLuongTon ?? 0))
                    {
                        TempData["Error"] = $"Số lượng vượt quá tồn kho! (Còn lại: {sanPham.SoLuongTon})";
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
                        gioHang.SoLuong += requestedQuantity;
                        gioHang.Gia = sanPham.Gia ?? 0;
                    }

                    // Giảm số lượng tồn kho trong SanPham
                    sanPham.SoLuongTon = (sanPham.SoLuongTon ?? 0) - requestedQuantity;
                    if (sanPham.SoLuongTon < 0) sanPham.SoLuongTon = 0;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Debug.WriteLine($"AddToCart Saved - MaSP: {gioHang.MaSp}, MaTk: {gioHang.MaTk}, SoLuong: {gioHang.SoLuong}, SoLuongTon: {sanPham.SoLuongTon}");

                    TempData["Success"] = "Thêm vào giỏ hàng thành công!";
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"AddToCart Error: {ex.Message}");
                    TempData["Error"] = "Có lỗi xảy ra khi thêm vào giỏ hàng.";
                    return RedirectToAction("Index", "SanPham");
                }
            }
        }

        // GET: GioHang/Cart
        public async Task<IActionResult> Cart()
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Index", "Account");
            }

            var gioHang = await _context.GioHangs
                .Include(g => g.MaSpNavigation)
                .Where(g => g.MaTk == maKhachHang)
                .ToListAsync();

            var availableQuantities = new Dictionary<int, int>();
            foreach (var item in gioHang)
            {
                if (item.MaSpNavigation == null)
                {
                    TempData["Error"] = $"Không thể tải thông tin sản phẩm (MaSP: {item.MaSp})";
                    return RedirectToAction("Index", "SanPham");
                }

                // Lấy trực tiếp SoLuongTon từ SanPham
                int availableQuantity = item.MaSpNavigation.SoLuongTon ?? 0;
                availableQuantities[item.MaSp] = availableQuantity > 0 ? availableQuantity : 0;

                Debug.WriteLine($"Cart - MaSP: {item.MaSp}, MaTk: {maKhachHang}, SoLuongTon: {item.MaSpNavigation.SoLuongTon}, Available: {availableQuantity}");
            }

            ViewBag.AvailableQuantities = availableQuantities;
            return View("~/Views/SanPham/Cart.cshtml", gioHang);
        }

        // POST: GioHang/IncreaseQuantity
        [HttpPost]
        public async Task<IActionResult> IncreaseQuantity(int maSP)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Index", "Account");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var gioHang = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
                    var sanPham = await _context.SanPhams.FindAsync(maSP);

                    if (gioHang != null && sanPham != null)
                    {
                        // Kiểm tra số lượng tồn kho
                        if ((sanPham.SoLuongTon ?? 0) < 1)
                        {
                            TempData["Error"] = $"Số lượng tồn không đủ! (Còn lại: {sanPham.SoLuongTon})";
                            return RedirectToAction("Cart");
                        }

                        gioHang.SoLuong++;
                        sanPham.SoLuongTon = (sanPham.SoLuongTon ?? 0) - 1;
                        if (sanPham.SoLuongTon < 0) sanPham.SoLuongTon = 0;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        TempData["Success"] = "Tăng số lượng thành công!";
                    }
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"IncreaseQuantity Error: {ex.Message}");
                    TempData["Error"] = "Có lỗi xảy ra khi tăng số lượng.";
                    return RedirectToAction("Cart");
                }
            }
        }

        // POST: GioHang/DecreaseQuantity
        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(int maSP)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Index", "Account");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var gioHang = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
                    var sanPham = await _context.SanPhams.FindAsync(maSP);

                    if (gioHang != null && gioHang.SoLuong > 1 && sanPham != null)
                    {
                        gioHang.SoLuong--;
                        sanPham.SoLuongTon = (sanPham.SoLuongTon ?? 0) + 1;
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        TempData["Success"] = "Giảm số lượng thành công!";
                    }
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"DecreaseQuantity Error: {ex.Message}");
                    TempData["Error"] = "Có lỗi xảy ra khi giảm số lượng.";
                    return RedirectToAction("Cart");
                }
            }
        }

        // POST: GioHang/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int maSP)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Index", "Account");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var gioHang = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
                    var sanPham = await _context.SanPhams.FindAsync(maSP);

                    if (gioHang != null && sanPham != null)
                    {
                        int removedQuantity = gioHang.SoLuong ?? 0;
                        _context.GioHangs.Remove(gioHang);
                        sanPham.SoLuongTon = (sanPham.SoLuongTon ?? 0) + removedQuantity;
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        TempData["Success"] = "Xóa sản phẩm khỏi giỏ hàng thành công!";
                    }
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"RemoveFromCart Error: {ex.Message}");
                    TempData["Error"] = "Có lỗi xảy ra khi xóa sản phẩm.";
                    return RedirectToAction("Cart");
                }
            }
        }

        // POST: GioHang/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int maSP, int soLuong)
        {
            int maKhachHang = GetCurrentUserId();
            if (maKhachHang == 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Index", "Account");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var gioHang = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
                    var sanPham = await _context.SanPhams.FindAsync(maSP);

                    if (gioHang != null && sanPham != null)
                    {
                        int currentQuantity = gioHang.SoLuong ?? 0;
                        int quantityChange = soLuong - currentQuantity;

                        // Kiểm tra số lượng tồn kho khả dụng
                        if (quantityChange > (sanPham.SoLuongTon ?? 0))
                        {
                            TempData["Error"] = $"Số lượng không hợp lệ hoặc vượt quá tồn kho! (Còn lại: {sanPham.SoLuongTon})";
                            return RedirectToAction("Cart");
                        }

                        if (soLuong > 0)
                        {
                            gioHang.SoLuong = soLuong;
                            sanPham.SoLuongTon = (sanPham.SoLuongTon ?? 0) - quantityChange;
                            if (sanPham.SoLuongTon < 0) sanPham.SoLuongTon = 0;
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            TempData["Success"] = "Cập nhật số lượng thành công!";
                        }
                        else
                        {
                            TempData["Error"] = "Số lượng không hợp lệ!";
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Sản phẩm không tồn tại trong giỏ hàng!";
                    }
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"UpdateQuantity Error: {ex.Message}");
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật số lượng.";
                    return RedirectToAction("Cart");
                }
            }
        }

        private int GetCurrentUserId()
        {
            var maTk = HttpContext.Session.GetString("MaTk");
            Debug.WriteLine($"GetCurrentUserId - MaTk: {maTk}");

            if (string.IsNullOrEmpty(maTk))
            {
                return 0; // Yêu cầu đăng nhập
            }

            return Convert.ToInt32(maTk);
        }
    }
}