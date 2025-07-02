using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class Musica
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string RutaArchivo { get; set; }
        public string Duracion { get; set; }
        public DateTime FechaSubida { get; set; }
        public string? Letra { get; set; }
        public bool EsActiva { get; set; }
        public double CalificacionPromedio { get; set; }
        public int TotalCalificaciones { get; set; }
        public int Reproducciones { get; set; }
        public int Descargas { get; set; }

        // Foreign Keys
        public string ArtistaId { get; set; }
        public int? AlbumId { get; set; }

        // Relaciones
        public User Artista { get; set; }
        public Album Album { get; set; }
        public List<PlaylistMusica>? PlaylistMusicas { get; set; }
        public List<FavoritoMusica>? FavoritoMusicas { get; set; }
        public List<Calificacion>? Calificaciones { get; set; }
    }
}
