using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class ChiTietDonHang
{
    public int MaDonHang { get; set; }

    public int MaSp { get; set; }

    public int SoLuong { get; set; }

    public int? Gia { get; set; }

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}
