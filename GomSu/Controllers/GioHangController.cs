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
                    int currentQuantity = gioHang?.SoLuong ?? 0;
                    int newTotalQuantity = currentQuantity + requestedQuantity;

                    // Calculate available quantity based on SoLuongTon and current cart quantity
                    int availableQuantity = sanPham.SoLuongTon ?? 0;

                    // Log the values for debugging
                    Debug.WriteLine($"AddToCart - MaSP: {maSP}, SoLuongTon: {sanPham.SoLuongTon}, Available: {availableQuantity}, Current: {currentQuantity}, Requested: {newTotalQuantity}");

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

                    // Reduce SoLuongTon
                    sanPham.SoLuongTon -= requestedQuantity;
                    if (sanPham.SoLuongTon < 0) sanPham.SoLuongTon = 0;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Thêm vào giỏ hàng thành công!";
                    return RedirectToAction("Cart");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Có lỗi xảy ra khi thêm vào giỏ hàng.";
                    return RedirectToAction("Index", "SanPham");
                }
            }
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

            // Calculate available quantities for each product using SoLuongTon directly
            var availableQuantities = new Dictionary<int, int>();
            foreach (var item in gioHang)
            {
                if (item.MaSpNavigation == null)
                {
                    TempData["Error"] = $"Không thể tải thông tin sản phẩm (MaSP: {item.MaSp})";
                    return RedirectToAction("Index", "SanPham");
                }

                int availableQuantity = item.MaSpNavigation.SoLuongTon ?? 0;
                availableQuantities[item.MaSp] = availableQuantity;

                // Log the values for debugging
                Debug.WriteLine($"Cart - MaSP: {item.MaSp}, SoLuongTon: {item.MaSpNavigation.SoLuongTon}, Available: {availableQuantity}");
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
                        int availableQuantity = sanPham.SoLuongTon ?? 0;

                        // Log the values for debugging
                        Debug.WriteLine($"IncreaseQuantity - MaSP: {maSP}, SoLuongTon: {sanPham.SoLuongTon}, Available: {availableQuantity}, Current: {currentQuantity}, NewQuantity: {currentQuantity + 1}");

                        if (currentQuantity + 1 <= availableQuantity)
                        {
                            gioHang.SoLuong++;
                            sanPham.SoLuongTon -= 1;
                            if (sanPham.SoLuongTon < 0) sanPham.SoLuongTon = 0;

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            TempData["Success"] = "Tăng số lượng thành công!";
                        }
                        else
                        {
                            TempData["Error"] = $"Số lượng tồn không đủ! (Còn lại: {availableQuantity})";
                        }
                    }
                    return RedirectToAction("Cart");
                }
                catch
                {
                    await transaction.RollbackAsync();
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
            if (maKhachHang == 0) return Unauthorized();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var gioHang = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
                    var sanPham = await _context.SanPhams.FindAsync(maSP);

                    if (gioHang != null && sanPham != null && gioHang.SoLuong > 1)
                    {
                        gioHang.SoLuong--;
                        sanPham.SoLuongTon += 1;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        TempData["Success"] = "Giảm số lượng thành công!";
                    }
                    return RedirectToAction("Cart");
                }
                catch
                {
                    await transaction.RollbackAsync();
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
            if (maKhachHang == 0) return Unauthorized();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var gioHang = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);
                    var sanPham = await _context.SanPhams.FindAsync(maSP);

                    if (gioHang != null && sanPham != null)
                    {
                        sanPham.SoLuongTon += gioHang.SoLuong;
                        _context.GioHangs.Remove(gioHang);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        TempData["Success"] = "Xóa sản phẩm khỏi giỏ hàng thành công!";
                    }
                    return RedirectToAction("Cart");
                }
                catch
                {
                    await transaction.RollbackAsync();
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
            if (maKhachHang == 0) return Unauthorized();

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
                        int availableQuantity = sanPham.SoLuongTon ?? 0;

                        // Log the values for debugging
                        Debug.WriteLine($"UpdateQuantity - MaSP: {maSP}, SoLuongTon: {sanPham.SoLuongTon}, Available: {availableQuantity}, Current: {currentQuantity}, Requested: {soLuong}");

                        if (soLuong > 0 && soLuong <= availableQuantity)
                        {
                            int quantityDifference = soLuong - currentQuantity;
                            sanPham.SoLuongTon -= quantityDifference;
                            if (sanPham.SoLuongTon < 0) sanPham.SoLuongTon = 0;

                            gioHang.SoLuong = soLuong;
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
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
                catch
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật số lượng.";
                    return RedirectToAction("Cart");
                }
            }
        }

        private int GetCurrentUserId()
        {
            var maTk = HttpContext.Session.GetString("MaTk");
            return string.IsNullOrEmpty(maTk) ? 0 : Convert.ToInt32(maTk);
        }
    }
}