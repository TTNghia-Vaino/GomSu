using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GomSu.Models;

namespace GomSu.Components
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly GomsuContext _context;

        public CartBadgeViewComponent(GomsuContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var maTk = HttpContext.Session.GetString("MaTk");
            int totalItems = 0;
            if (!string.IsNullOrEmpty(maTk))
            {
                totalItems = await _context.GioHangs
                    .Where(g => g.MaTk == Convert.ToInt32(maTk))
                    .SumAsync(g => g.SoLuong) ?? 0;
            }
            return View(totalItems);
        }
    }
}