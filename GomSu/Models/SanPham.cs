using System;
using System.Collections.Generic;

namespace GomSu.Models
{
    public partial class SanPham
    {
        public SanPham()
        {
            DanhGiaSanPhams = new HashSet<DanhGiaSanPham>();
            GioHangs = new HashSet<GioHang>();
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
        }

        public int MaSP { get; set; }
        public string TenSP { get; set; } = null!;
        public int Gia { get; set; }
        public string? HinhAnh { get; set; }
        public string MaLoaiSP { get; set; } = null!;
        public string? MoTa { get; set; }
        public int SoLuongTon { get; set; }

        // Navigation Property
        public virtual LoaiSanPham MaLoaiSPNavigation { get; set; } = null!;
        public virtual ICollection<DanhGiaSanPham> DanhGiaSanPhams { get; set; }
        public virtual ICollection<GioHang> GioHangs { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}
