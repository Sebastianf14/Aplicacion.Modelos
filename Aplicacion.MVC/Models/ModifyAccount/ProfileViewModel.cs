using System.ComponentModel.DataAnnotations;

namespace Aplicacion.MVC.Models.ModifyAccount
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Nombre")]
        public string?   FirstName { get; set; }

        [Required]
        [Display(Name = "Apellido")]
        public string? LastName { get; set; }

        // Para mostrar en la vista (no se valida)
        public string? CurrentEmail { get; set; }
    }
}
