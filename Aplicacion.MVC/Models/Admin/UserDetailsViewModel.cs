using Aplicacion.Modelos;

namespace Aplicacion.MVC.Models.Admin
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Role { get; set; }

        // Estadísticas de artista
        public int MusicCount { get; set; }
        public int AlbumCount { get; set; }
        public int FollowerCount { get; set; }
        public List<Musica> RecentMusics { get; set; } = new List<Musica>();

        // Estadísticas de usuario
        public int PlaylistCount { get; set; }
        public int DownloadCount { get; set; }
        public int FollowingCount { get; set; }
        public string SubscriptionPlan { get; set; }
        public string SubscriptionStatus { get; set; }
    }
}
