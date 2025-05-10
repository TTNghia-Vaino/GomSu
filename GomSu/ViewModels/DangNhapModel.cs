using System.ComponentModel.DataAnnotations;

namespace GomSu.ViewModels
{
    public class DangNhapModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string? TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string? MatKhau { get; set; }
    }
}
