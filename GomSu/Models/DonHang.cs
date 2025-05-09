using System;
using System.Collections.Generic;

namespace GomSu.Models
{
    public partial class DonHang
    {
        public DonHang()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
        }

        public int MaDonHang { get; set; }
        public int MaKhachHang { get; set; }
        public int? MaVoucher { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = null!;

        public virtual TaiKhoan MaKhachHangNavigation { get; set; } = null!;
        public virtual Voucher? MaVoucherNavigation { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}