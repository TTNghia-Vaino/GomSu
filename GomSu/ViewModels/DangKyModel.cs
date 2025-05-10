using System.ComponentModel.DataAnnotations;

namespace GomSu.ViewModels
{
    public class DangKyModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string? HoTen { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải đúng 10 ký tự")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại chỉ được chứa chữ số")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string? TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string? MatKhau { get; set; }
    }
}
