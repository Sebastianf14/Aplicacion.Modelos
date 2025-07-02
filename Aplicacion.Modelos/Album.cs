namespace Aplicacion.Modelos
{
    public class Album
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaLanzamiento { get; set; }
        public string RutaPortada { get; set; }
        public bool EsActivo { get; set; }

        // Foreign Key
        public string ArtistaId { get; set; }

        // Relaciones
        public User Artista { get; set; }
        public List<Musica>? Musicas { get; set; }
    }
}
