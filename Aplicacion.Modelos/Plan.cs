using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class Plan
    {
        [Key] public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public double Precio { get; set; }
        public bool PermisoDescarga { get; set; }
        public int DispositivosPermitidos { get; set; }
        public bool EsActivo { get; set; }

        public List<User>? Usuarios { get; set; }
        public List<Suscripcion>? Suscripciones { get; set; }
    }
}
