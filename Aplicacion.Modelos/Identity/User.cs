using Aplicacion.Modelos.Favorite;
using Aplicacion.Modelos.Implementations;
using Aplicacion.Modelos.Suscription;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos.Identity
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Artist 
        public List<Musica>? Musics { get; set; }
        public List<Album>? Albums { get; set; }
        public List<Playlist>? Playlists { get; set; }
        public List<UserSubscription>? Subscriptions { get; set; }
        public List<FavoritoArtista>? FavoriteArtists { get; set; }
        public List<FavoritoMusica>? FavoriteMusics { get; set; }
        public List<Notificacion>? Notifications { get; set; }
        public List<Download>? Downloads { get; set; }

        [InverseProperty("Follower")]
        public List<Follow>? Following { get; set; } // Artistas que sigue

        [InverseProperty("Artist")]
        public List<Follow>? Followers { get; set; }
    }

}
