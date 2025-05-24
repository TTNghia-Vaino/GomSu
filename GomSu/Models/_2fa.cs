using System;
using System.Collections.Generic;

namespace GomSu.Models;

public partial class _2fa
{
    public int MaTk { get; set; }

    public string Code { get; set; } = null!;

    public DateTime BatDau { get; set; }

    public DateTime KetThuc { get; set; }

    public bool DaDung { get; set; }

    public virtual TaiKhoan MaTkNavigation { get; set; } = null!;
}
