using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class TaiKhoan
{
    public int MaTk { get; set; }

    public string? HoTen { get; set; }

    public string? TenDangNhap { get; set; }

    public string? MatKhau { get; set; }

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public DateOnly? NgayTao { get; set; }

    public int? Quyen { get; set; }

    public virtual ICollection<DanhGiaSanPham> DanhGiaSanPhams { get; set; } = new List<DanhGiaSanPham>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
}
