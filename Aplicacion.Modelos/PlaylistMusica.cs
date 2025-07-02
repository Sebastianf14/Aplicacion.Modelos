using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class PlaylistMusica
    {
        public int Id { get; set; }

        public int PlaylistId { get; set; }
        public int MusicaId { get; set; }
        // Relaciones
        public Playlist Playlist { get; set; }
        public Musica Musica { get; set; }
    }
}
