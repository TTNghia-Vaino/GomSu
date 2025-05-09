using System;
using System.Collections.Generic;

namespace GomSu.Models
{
    public partial class Voucher
    {
        public Voucher()
        {
            DonHangs = new HashSet<DonHang>();
        }

        public int MaVoucher { get; set; }
        public string MaVoucherNavigation { get; set; } = null!;
        public DateTime NgayBatDau { get; set; } // Add if exists
        public DateTime NgayKetThuc { get; set; } // Add if exists
        public virtual ICollection<DonHang> DonHangs { get; set; } // One-to-many with DonHang
    }
}