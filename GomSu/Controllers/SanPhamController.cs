using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GomSu.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

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

            // Lấy trực tiếp SoLuongTon từ SanPham
            var availableQuantities = new Dictionary<int, int>();
            foreach (var item in sanPhams)
            {
                int availableQuantity = item.SoLuongTon ?? 0;
                availableQuantities[item.MaSp] = availableQuantity > 0 ? availableQuantity : 0;
            }
            ViewBag.AvailableQuantities = availableQuantities;

            return View(sanPhams);
        }

        // GET: SanPham/Details/5
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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

            // Lấy trực tiếp SoLuongTon từ SanPham
            int availableQuantity = sanPham.SoLuongTon ?? 0;
            ViewBag.AvailableQuantity = availableQuantity > 0 ? availableQuantity : 0;

            return View(sanPham);
        }
    }
}