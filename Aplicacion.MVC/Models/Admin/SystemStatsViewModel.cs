using Aplicacion.Modelos;
using Aplicacion.Modelos.Suscription;

namespace Aplicacion.MVC.Models.Admin
{
    public class SystemStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalArtists { get; set; }
        public int TotalMusics { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalPlaylists { get; set; }
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int PremiumSubscriptions { get; set; }
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int TodayNotifications { get; set; }
        public List<Musica> RecentMusics { get; set; } = new();
        public List<UserSubscription> RecentSubscriptions { get; set; } = new();
    }
}
