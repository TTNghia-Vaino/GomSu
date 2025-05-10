using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class SanPham
{
    public int MaSp { get; set; }

    public string? TenSp { get; set; }

    public int? Gia { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public int? SoLuongTon { get; set; }

    public string? MaLoaiSp { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<DanhGiaSanPham> DanhGiaSanPhams { get; set; } = new List<DanhGiaSanPham>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual LoaiSanPham? MaLoaiSpNavigation { get; set; }
}
