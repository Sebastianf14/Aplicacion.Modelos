using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class User : IdentityUser
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaRegistri { get; set; }

        public int PlanId { get; set; }
        public Plan Plan { get; set; } //

        // Usuario
        public List<Playlist>? Playlists { get; set; }
        public List<FavoritoMusica>? FavoritoMusicas { get; set; }
        public List<FavoritoArtista>? FavoritoArtistas { get; set; }
        public List<Calificacion>? Calificaciones { get; set; }
        public List<Suscripcion>? Suscripciones { get; set; }
        public List<Notificacion>? Notificaciones { get; set; }

        // artistas
        public List<Musica>? Musicas { get; set; }
        public List<Album>? Albumes { get; set; }
    }
}
