using Aplicacion.Modelos.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos.Favorite
{
    public class FavoritoArtista
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ArtistId { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Navegation
        public User? User { get; set; }
        public User? Artist { get; set; }
    }
}
