using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class DanhGiaSanPham
{
    public int MaDanhGia { get; set; }

    public int? MaSp { get; set; }

    public int? MaTk { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateOnly? NgayDanhGia { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }

    public virtual TaiKhoan? MaTkNavigation { get; set; }
}
