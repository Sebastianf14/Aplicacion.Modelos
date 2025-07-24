using Aplicacion.API.Consumer;
using Aplicacion.Modelos;
using Aplicacion.Modelos.Identity;
using Aplicacion.Modelos.Implementations;
using Microsoft.AspNetCore.Identity;

namespace Aplicacion.MVC.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message, Notificacion.NotificationType type, string actionUrl = null, int? relatedEntityId = null);
        Task NotifyFollowersAsync(int artistId, string musicTitle, int musicId);
        Task NotifyArtistNewFollowerAsync(int artistId, string followerName);
        Task NotifyWelcomeAsync(int userId, string userName);
        Task NotifyUserSubscriptionAsync(int userId, string planName);
        Task NotifySubscriptionExpiringAsync(int userId, string planName, DateTime expiryDate);
    }

    public class NotificationService : INotificationService
    {

        private readonly UserManager<User> _userManager; 

        public NotificationService(UserManager<User> userManager) 
        {
            _userManager = userManager;
        }
        public async Task CreateNotificationAsync(int userId, string title, string message, Notificacion.NotificationType type, string actionUrl = null, int? relatedEntityId = null)
        {
            try
            {
                var notification = new Notificacion
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    CreatedDate = DateTime.Now,
                    IsRead = false,
                    ActionUrl = actionUrl,
                    RelatedEntityId = relatedEntityId
                };

                Crud<Notificacion>.Create(notification);
            }
            catch (Exception ex)
            {
                // Log error pero no romper la aplicación
                System.Diagnostics.Debug.WriteLine($"Error creando notificación: {ex.Message}");
            }
        }

        public async Task NotifyFollowersAsync(int artistId, string musicTitle, int musicId)
        {
            try
            {
                // Obtener seguidores del artista
                var follows = Crud<Follow>.GetAll().Where(f => f.ArtistId == artistId).ToList();
                var artist = Crud<Musica>.GetAll().FirstOrDefault(m => m.ArtistId == artistId)?.Artist;
                var artistName = $"{artist?.FirstName} {artist?.LastName}".Trim();

                foreach (var follow in follows)
                {
                    await CreateNotificationAsync(
                        follow.FollowerId,
                        "Nueva música disponible",
                        $"{artistName} subió la canción '{musicTitle}'",
                        Notificacion.NotificationType.NewMusic,
                        $"/Musics/Details/{musicId}",
                        musicId
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notificando seguidores: {ex.Message}");
            }
        }

        public async Task NotifyArtistNewFollowerAsync(int artistId, string followerName)
        {
            await CreateNotificationAsync(
                artistId,
                "Nuevo seguidor",
                $"{followerName} comenzó a seguirte",
                Notificacion.NotificationType.System,
                "/Follows/Followers"
            );
        }

        public async Task NotifyWelcomeAsync(int userId, string userName)
        {
            await CreateNotificationAsync(
                userId,
                "¡Bienvenido a SonikoMusic!",
                $"Hola {userName}, descubre música increíble y sigue a tus artistas favoritos",
                Notificacion.NotificationType.Welcome,
                "/Musics/Search"
            );
        }


        public async Task NotifyUserSubscriptionAsync(int userId, string planName)
        {
            try
            {
                await CreateNotificationAsync(
                    userId,
                    "¡Suscripción activada!",
                    $"Tu plan {planName} está ahora activo. Disfruta de todos los beneficios premium",
                    Notificacion.NotificationType.System,
                    "/UserSubscriptions/Index"
                );
                Console.WriteLine($" Notificación de suscripción enviada al usuario ID: {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error notificando suscripción: {ex.Message}");
            }
        }

        public async Task NotifySubscriptionExpiringAsync(int userId, string planName, DateTime expiryDate)
        {
            var daysLeft = (expiryDate - DateTime.Now).Days;
            await CreateNotificationAsync(
                userId,
                "Tu suscripción expira pronto",
                $"Tu plan {planName} expira en {daysLeft} días. Renueva para seguir disfrutando todos los beneficios",
                Notificacion.NotificationType.SubscriptionExpiry,
                "/UserSubscriptions/Plans"
            );
        }
    }
}
