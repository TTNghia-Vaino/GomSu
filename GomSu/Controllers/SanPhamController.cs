using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GomSu.Models;
using System.Threading.Tasks;

namespace GomSu.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly GomsuContext _context;

        public SanPhamController(GomsuContext context)
        {
            _context = context;
        }

        // GET: SanPham/Index?maLoaiSP={id}
        public async Task<IActionResult> Index(string maLoaiSP = null)
        {
            var sanPhams = string.IsNullOrEmpty(maLoaiSP)
                ? await _context.SanPhams
                    .Include(s => s.MaLoaiSpNavigation)
                    .ToListAsync()
                : await _context.SanPhams
                    .Include(s => s.MaLoaiSpNavigation)
                    .Where(s => s.MaLoaiSp == maLoaiSP)
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
                .AsNoTracking()
                .Include(s => s.MaLoaiSpNavigation)
                .Include(s => s.DanhGiaSanPhams)
                .ThenInclude(d => d.MaTkNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            // Calculate total quantity in all carts for this product
            int totalCartQuantity = await _context.GioHangs
                .Where(g => g.MaSp == id)
                .SumAsync(g => g.SoLuong ?? 0);

            int availableQuantity = (sanPham.SoLuongTon ?? 0) - totalCartQuantity;
            ViewBag.AvailableQuantity = availableQuantity;

            return View(sanPham);
        }
    }
}