using Aplicacion.Modelos.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos.Implementations
{
    public class Follow
    {
        public int Id { get; set; }
        public int FollowerId { get; set; } // Usuario que sigue
        public int ArtistId { get; set; }   // Artista seguido
        public DateTime FollowDate { get; set; } = DateTime.Now;

        // Navegación
        public User? Follower { get; set; }
        public User? Artist { get; set; }
    }
}
