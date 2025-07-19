using Aplicacion.Modelos.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos.Favorite
{
    public class FavoritoMusica
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MusicId { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Navegación
        public User? User { get; set; }
        public Musica? Music { get; set; }
    }
}
