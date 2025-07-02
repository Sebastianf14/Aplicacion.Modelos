using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class FavoritoArtista
    {
        public int Id { get; set; }
        public DateTime FechaAgregada { get; set; }

        public string UsuarioId { get; set; }
        public string ArtistaId { get; set; }
        // Relaciones
        public User Usuario { get; set; }
        public User Artista { get; set; }
    }
}
