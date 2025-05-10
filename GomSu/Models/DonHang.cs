using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class DonHang
{
    public int MaDonHang { get; set; }

    public int? MaTk { get; set; }

    public DateOnly? NgayDatHang { get; set; }

    public int? TongTien { get; set; }

    public string? TrangThai { get; set; }

    public int? PhuongThucThanhToan { get; set; }

    public int? TongTienVoucher { get; set; }

    public string? MaVoucher { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual TaiKhoan? MaTkNavigation { get; set; }

    public virtual Voucher? MaVoucherNavigation { get; set; }
}
