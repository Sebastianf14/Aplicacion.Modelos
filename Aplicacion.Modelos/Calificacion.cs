using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class Calificacion
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public DateTime FechaCalificacion { get; set; }

        public int MusicaId { get; set; }
        public int Puntuacion { get; set; }
        // Relaciones
        public User Usuario { get; set; }
        public Musica Musica { get; set; }
    }
}
