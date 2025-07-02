using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class Notificacion
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool EsLeida { get; set; }
        public string Razon { get; set; }

        public string UsuarioId { get; set; }
        // Relaciones
        public User Usuario { get; set; }
    }
}
