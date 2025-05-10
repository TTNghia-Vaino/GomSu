using System.ComponentModel.DataAnnotations;

namespace GomSu.ViewModels
{
    public class QuenMatKhauModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
