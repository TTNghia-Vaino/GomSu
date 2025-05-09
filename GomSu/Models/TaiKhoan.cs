using System;
using System.Collections.Generic;

namespace GomSu.Models
{
    public partial class TaiKhoan
    {
        public TaiKhoan()
        {
            DonHangs = new HashSet<DonHang>();
            GioHangs = new HashSet<GioHang>();
            DanhGiaSanPhams = new HashSet<DanhGiaSanPham>();
        }

        public int MaTK { get; set; }
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        // Removed DiaChi and VaiTro since they are not in the database

        public virtual ICollection<DonHang> DonHangs { get; set; }
        public virtual ICollection<GioHang> GioHangs { get; set; }
        public virtual ICollection<DanhGiaSanPham> DanhGiaSanPhams { get; set; }
    }
}