using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class LoaiSanPham
{
    public string MaLoaiSp { get; set; } = null!;

    public string? TenLoaiSp { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
