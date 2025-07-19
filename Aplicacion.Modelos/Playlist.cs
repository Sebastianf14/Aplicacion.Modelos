using Aplicacion.Modelos.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsPublic { get; set; } = true;

        // Usuario                                                       d
        public int UserId { get; set; }
        public User? User { get; set; }


        public List<PlaylistMusica>? PlaylistMusics { get; set; }
    }
}
