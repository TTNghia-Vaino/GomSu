using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GomSu.Models;
using System.Threading.Tasks;
using System.Linq;

namespace GomSu.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly GomsuContext _context;

        public SanPhamController(GomsuContext context)
        {
            _context = context;
        }

        // GET: SanPham
        public async Task<IActionResult> Index()
        {
            var sanPhams = await _context.SanPhams
                .Include(s => s.MaLoaiSpNavigation)
                .ToListAsync();
            return View(sanPhams);
        }

        // GET: SanPham/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.MaLoaiSpNavigation)
                .Include(s => s.DanhGiaSanPhams)
                .ThenInclude(d => d.MaTkNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        //// POST: SanPham/AddToCart
        //[HttpPost]
        //public async Task<IActionResult> AddToCart(int maSP, int? soLuong)
        //{
        //    int maKhachHang = 1; // Replace with actual user ID from authentication

        //    var sanPham = await _context.SanPhams.FindAsync(maSP);
        //    if (sanPham == null || sanPham.SoLuongTon < (soLuong ?? 1))
        //    {
        //        return BadRequest("Sản phẩm không tồn tại hoặc số lượng không đủ.");
        //    }

        //    var gioHang = await _context.GioHangs
        //        .FirstOrDefaultAsync(g => g.MaTk == maKhachHang && g.MaSp == maSP);

        //    if (gioHang == null)
        //    {
        //        gioHang = new GioHang
        //        {
        //            MaKhachHang = maKhachHang,
        //            MaSP = maSP,
        //            SoLuong = soLuong ?? 1,
        //            Gia = (decimal)sanPham.Gia
        //        };
        //        _context.GioHangs.Add(gioHang);
        //    }
        //    else
        //    {
        //        gioHang.SoLuong += (int)(soLuong ?? 1); // Explicit cast to int
        //        gioHang.Gia = (decimal)sanPham.Gia;
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Cart");
        //}

        //// GET: SanPham/Cart
        //public async Task<IActionResult> Cart()
        //{
        //    int maKhachHang = 1; // Replace with actual user ID
        //    var gioHang = await _context.GioHangs
        //        .Include(g => g.MaSPNavigation)
        //        .Where(g => g.MaKhachHang == maKhachHang)
        //        .ToListAsync();
        //    return View(gioHang);
        //}
    }
}