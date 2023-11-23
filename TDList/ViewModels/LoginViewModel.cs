using System.ComponentModel.DataAnnotations;

namespace TDList.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email или логин ")]
        public string? Email { get; set; }
        [Display(Name = "Логин или Email")]
        public string? Login { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
        [Display(Name = "Запомнить?")]
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
