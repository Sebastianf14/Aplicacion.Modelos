using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool EsPublica { get; set; }

        // Foreign Key
        public string UsuarioId { get; set; }

        // Relaciones
        public User Usuario { get; set; }
        public List<PlaylistMusica>? PlaylistMusicas { get; set; }
    }
}
