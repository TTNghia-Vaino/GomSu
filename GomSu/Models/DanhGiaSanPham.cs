using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GomSu.Models
{
    public partial class DanhGiaSanPham
    {
        [Key]
        public int MaDanhGia { get; set; }

        public int? MaSP { get; set; }

        public int? MaTK { get; set; }

        public int? SoSao { get; set; }

        [StringLength(255)]
        public string? NoiDung { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NgayDanhGia { get; set; }

        [ForeignKey("MaSP")]
        [InverseProperty("DanhGiaSanPhams")]
        public virtual SanPham? MaSPNavigation { get; set; }

        [ForeignKey("MaTK")]
        [InverseProperty("DanhGiaSanPhams")]
        public virtual TaiKhoan? MaTKNavigation { get; set; }
    }
}