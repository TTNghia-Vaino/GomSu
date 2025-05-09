using System;
using System.Collections.Generic;

namespace GomSu.Models
{
    public partial class LoaiSanPham
    {
        public LoaiSanPham()
        {
            SanPhams = new HashSet<SanPham>();
        }

        public string MaLoaiSP { get; set; } = null!;  // Đổi int → string
        public string TenLoaiSP { get; set; } = null!;

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
