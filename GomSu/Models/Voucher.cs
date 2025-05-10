using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class Voucher
{
    public string MaVoucher { get; set; } = null!;

    public string? TenVoucher { get; set; }

    public int? GiamGia { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public string? DieuKien { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
