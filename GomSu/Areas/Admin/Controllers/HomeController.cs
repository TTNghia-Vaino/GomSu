using GomSu.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GomSu.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly GomsuContext _context;
        
        public HomeController(GomsuContext context)
        {
            _context = context;
        }

        [Area("Admin")]
        public IActionResult Index()
        {

            // Lấy thông tin tổng quan cho dashboard
            ViewBag.TongSanPham = _context.SanPhams.Count();
            ViewBag.TongDonHang = _context.DonHangs.Count();
            ViewBag.TongTaiKhoan = _context.TaiKhoans.Count();
            
            // Lấy danh sách đơn hàng gần đây (5 đơn hàng mới nhất)
            //ViewBag.DonHangGanDay = _context.DonHangs
            //    .OrderByDescending(d => d.NgayDat)
            //    .Take(5)
            //    .ToList();
            
            // Lấy danh sách sản phẩm bán chạy
            // Giả sử có bảng ChiTietDonHang liên kết với SanPham
            // ViewBag.SanPhamBanChay = _context.ChiTietDonHangs
            //     .GroupBy(c => c.SanPhamId)
            //     .Select(g => new { 
            //         SanPhamId = g.Key, 
            //         TongBan = g.Sum(c => c.SoLuong) 
            //     })
            //     .OrderByDescending(x => x.TongBan)
            //     .Take(5)
            //     .Join(_context.SanPhams, 
            //           x => x.SanPhamId, 
            //           s => s.Id, 
            //           (x, s) => new { 
            //               SanPham = s, 
            //               DaBan = x.TongBan 
            //           })
            //     .ToList();

            return View();
        }
    }
}
