using Aplicacion.Modelos.Identity;
using System.ComponentModel.DataAnnotations;

namespace Aplicacion.Modelos
{
    public class Album
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? CoverImagePath { get; set; }

        // Artista
        public int ArtistId { get; set; }
        public User? Artist { get; set; }


        public List<Musica>? Musics { get; set; }
    }
}
