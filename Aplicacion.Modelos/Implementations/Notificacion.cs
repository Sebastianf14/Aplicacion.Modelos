using Aplicacion.Modelos.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos.Implementations
{
    public class Notificacion
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string? ActionUrl { get; set; } // URL a donde redirigir al hacer click
        public int? RelatedEntityId { get; set; } // ID del objeto relacionado (Music, Album, etc)

        // Navegación
        public User? User { get; set; }

        public enum NotificationType
        {
            NewMusic = 1,        // "Tu artista favorito subió nueva música"
            NewAlbum = 2,        // "Nuevo album disponible"
            PlaylistUpdate = 3,  // "Alguien agregó música a tu playlist"
            SubscriptionExpiry = 4, // "Tu suscripción expira pronto"
            Welcome = 5,         // Mensaje de bienvenida
            System = 6           // Actualizaciones del sistema
        }
    }
}
