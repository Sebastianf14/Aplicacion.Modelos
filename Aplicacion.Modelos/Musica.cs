using Aplicacion.Modelos.Favorite;
using Aplicacion.Modelos.Identity;
using Aplicacion.Modelos.Implementations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class Musica
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Duration { get; set; }
        public MusicalGenre Genre { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public string? Description { get; set; }
        public string FilePath { get; set; }



        public int ArtistId { get; set; } // Arist
        public int? AlbumId { get; set; } //Album

        public User? Artist { get; set; }
        public Album? Album { get; set; }
        public List<PlaylistMusica>? PlaylistMusics { get; set; }
        public List<FavoritoMusica>? FavoriteMusics { get; set; }
        public List<Download>? Downloads { get; set; }


        // enums of Genres
        public enum MusicalGenre
        {
            Rock = 1,
            Pop = 2,
            Jazz = 3,
            Blues = 4,
            Classical = 5,
            Electronic = 6,
            HipHop = 7,
            Country = 8


        }
    }
}
