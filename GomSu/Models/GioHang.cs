using System;

namespace GomSu.Models
{
    public partial class GioHang
    {
        public GioHang()
        {
            // No navigation collections to initialize
        }

        public int MaKhachHang { get; set; }
        public int MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }

        public virtual TaiKhoan? MaKhachHangNavigation { get; set; }
        public virtual SanPham? MaSPNavigation { get; set; }
    }
}