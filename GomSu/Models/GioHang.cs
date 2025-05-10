using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class GioHang
{
    public int MaTk { get; set; }

    public int MaSp { get; set; }

    public int? SoLuong { get; set; }

    public int? Gia { get; set; }

    public virtual SanPham MaSpNavigation { get; set; } = null!;

    public virtual TaiKhoan MaTkNavigation { get; set; } = null!;
}
