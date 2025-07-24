using System.ComponentModel.DataAnnotations;

namespace Aplicacion.MVC.Models
{
    public class PaymentViewModel
    {
        public int PlanId { get; set; }
        public int Months { get; set; }
        public decimal TotalAmount { get; set; }
        public string PlanName { get; set; }

        [Required(ErrorMessage = "El nombre del titular es requerido")]
        [Display(Name = "Nombre del titular")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "El número de tarjeta es requerido")]
        [Display(Name = "Número de tarjeta")]
        [RegularExpression(@"^[0-9]{13,19}$", ErrorMessage = "El número de tarjeta debe tener entre 13 y 19 dígitos")] 
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "El mes de expiración es requerido")]
        [Display(Name = "Mes de expiración")]
        [Range(1, 12, ErrorMessage = "Mes inválido")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "El año de expiración es requerido")]
        [Display(Name = "Año de expiración")]
        [Range(2024, 2040, ErrorMessage = "Año inválido")]
        public int ExpiryYear { get; set; }

        [Required(ErrorMessage = "El CVV es requerido")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV inválido")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email de confirmación")]
        public string Email { get; set; }
    }
}
