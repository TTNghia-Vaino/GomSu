using System;
using System.ComponentModel.DataAnnotations;

namespace GomSu.Models
{
    public partial class ChiTietDonHang
    {
        public ChiTietDonHang()
        {
            // No navigation collections to initialize
        }

        [Key]
        public int MaDonHang { get; set; }
        [Key]
        public int MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }

        public virtual DonHang? MaDonHangNavigation { get; set; }
        public virtual SanPham? MaSanPhamNavigation { get; set; }
    }
}