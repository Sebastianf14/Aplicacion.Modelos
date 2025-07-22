using System.ComponentModel.DataAnnotations;

namespace Aplicacion.MVC.Models.Email
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
