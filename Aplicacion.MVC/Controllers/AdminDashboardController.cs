using Aplicacion.API.Consumer;
using Aplicacion.Models;
using Aplicacion.Models.Implementations;
using Aplicacion.Models.Suscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aplicacion.MVC.Models.Admin;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "admins")]
    public class AdminDashboardController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                // Obtener estadísticas generales
                var stats = GetSystemStats();
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error cargando dashboard: " + ex.Message;
                return View(new SystemStatsViewModel());
            }
        }

        private SystemStatsViewModel GetSystemStats()
        {
            var allMusics = Crud<Music>.GetAll();
            var allAlbums = Crud<Album>.GetAll();
            var allPlaylists = Crud<Playlist>.GetAll();
            var allSubscriptions = Crud<UserSubscription>.GetAll();
            var allNotifications = Crud<Notification>.GetAll();

            // Obtener usuarios únicos desde las músicas (como proxy)
            var allUsers = allMusics.Select(m => m.Artist).Where(u => u != null).GroupBy(u => u.Id).Select(g => g.First()).ToList();

            return new SystemStatsViewModel
            {
                TotalUsers = allUsers.Count,
                TotalArtists = allUsers.Count, 
                TotalMusics = allMusics.Count,
                TotalAlbums = allAlbums.Count,
                TotalPlaylists = allPlaylists.Count,
                TotalSubscriptions = allSubscriptions.Count,
                ActiveSubscriptions = allSubscriptions.Count(s => s.IsActive),
                PremiumSubscriptions = allSubscriptions.Count(s => s.SubscriptionPlan?.Price > 0 && s.IsActive),
                TotalNotifications = allNotifications.Count,
                UnreadNotifications = allNotifications.Count(n => !n.IsRead),
                TodayNotifications = allNotifications.Count(n => n.CreatedDate.Date == DateTime.Today),
                RecentMusics = allMusics.OrderByDescending(m => m.UploadDate).Take(5).ToList(),
                RecentSubscriptions = allSubscriptions.OrderByDescending(s => s.StartDate).Take(5).ToList()
            };
        }
    }
}