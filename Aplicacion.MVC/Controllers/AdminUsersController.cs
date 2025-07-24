using Aplicacion.API.Consumer;
using Aplicacion.Modelos;
using Aplicacion.Modelos.Favorite;
using Aplicacion.Modelos.Identity;
using Aplicacion.Modelos.Implementations;
using Aplicacion.Modelos.Suscription;
using Aplicacion.MVC.Models;
using Aplicacion.MVC.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "admins")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        public AdminUsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: AdminUsers - Lista general
        public async Task<ActionResult> Index()
        {
            var users = await GetAllUsersWithRoles();

            // Excluir admin actual
            var currentUserEmail = User.Identity.Name;
            users = users.Where(u => u.Email != "dilanalbacura887@gmail.com" && u.Email != currentUserEmail).ToList();

            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalArtists = users.Count(u => u.Role == "artists");
            ViewBag.TotalRegularUsers = users.Count(u => u.Role == "users");

            return View(users);
        }

        // GET: AdminUsers/Artists - Solo artistas
        public async Task<ActionResult> Artists()
        {
            var users = await GetAllUsersWithRoles();
            var artists = users.Where(u => u.Role == "artists" && u.Email != "dilanalbacura887@gmail.com").ToList();

            // Agregar estadísticas de música para cada artista
            foreach (var artist in artists)
            {
                var artistMusics = Crud<Musica>.GetBy("artist", artist.Id);
                var artistAlbums = Crud<Album>.GetBy("artist", artist.Id);

                artist.MusicCount = artistMusics.Count;
                artist.AlbumCount = artistAlbums.Count;
                artist.TotalPlays = 0; // Placeholder para futuras métricas
            }

            ViewBag.TotalArtists = artists.Count;
            ViewBag.TotalSongs = artists.Sum(a => a.MusicCount);
            ViewBag.TotalAlbums = artists.Sum(a => a.AlbumCount);

            return View(artists);
        }

        // GET: AdminUsers/RegularUsers - Solo usuarios normales
        public async Task<ActionResult> RegularUsers()
        {
            var users = await GetAllUsersWithRoles();
            var regularUsers = users.Where(u => u.Role == "users" && u.Email != "dilanalbacura887@gmail.com").ToList();

            // Agregar estadísticas para cada usuario
            foreach (var user in regularUsers)
            {
                var userPlaylists = Crud<Playlist>.GetBy("user", user.Id);
                var userSubscription = GetActiveSubscription(user.Id);
                var userDownloads = Crud<Download>.GetBy("user", user.Id);

                user.PlaylistCount = userPlaylists.Count;
                user.SubscriptionPlan = userSubscription?.SubscriptionPlan?.Name ?? "Ninguno";
                user.DownloadCount = userDownloads.Count;
            }

            ViewBag.TotalUsers = regularUsers.Count;
            ViewBag.TotalPlaylists = regularUsers.Sum(u => u.PlaylistCount);
            ViewBag.PremiumUsers = regularUsers.Count(u => u.SubscriptionPlan != "Gratuito" && u.SubscriptionPlan != "Ninguno");

            return View(regularUsers);
        }

        // GET: AdminUsers/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null || user.Email == "dilanalbacura887@gmail.com")
            {
                TempData["Error"] = "Usuario no encontrado";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "users";

            var userDetails = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                Role = userRole
            };

            // Estadísticas según el rol
            if (userRole == "artists")
            {
                var artistMusics = Crud<Musica>.GetBy("artist", user.Id);
                var artistAlbums = Crud<Album>.GetBy("artist", user.Id);
                var artistFollowers = Crud<Follow>.GetBy("followers", user.Id);

                userDetails.MusicCount = artistMusics.Count;
                userDetails.AlbumCount = artistAlbums.Count;
                userDetails.FollowerCount = artistFollowers.Count;
                userDetails.RecentMusics = artistMusics.OrderByDescending(m => m.UploadDate).Take(5).ToList();
            }
            else
            {
                var userPlaylists = Crud<Playlist>.GetBy("user", user.Id);
                var userDownloads = Crud<Download>.GetBy("user", user.Id);
                var userFollowing = Crud<Follow>.GetBy("following", user.Id);
                var userSubscription = GetActiveSubscription(user.Id);

                userDetails.PlaylistCount = userPlaylists.Count;
                userDetails.DownloadCount = userDownloads.Count;
                userDetails.FollowingCount = userFollowing.Count;
                userDetails.SubscriptionPlan = userSubscription?.SubscriptionPlan?.Name ?? "Gratuito";
                userDetails.SubscriptionStatus = userSubscription?.Status.ToString() ?? "Ninguna";
            }

            return View(userDetails);
        }

        // GET: AdminUsers/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null || user.Email == "dilanalbacura887@gmail.com")
            {
                TempData["Error"] = "Usuario no encontrado o no se puede eliminar";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "users";

            var deleteModel = new UserDeleteViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = userRole,
                RegistrationDate = user.RegistrationDate
            };

            // Estadísticas de contenido a eliminar
            if (userRole == "artists")
            {
                deleteModel.MusicCount = Crud<Musica>.GetBy("artist", user.Id).Count;
                deleteModel.AlbumCount = Crud<Album>.GetBy("artist", user.Id).Count;
            }
            else
            {
                deleteModel.PlaylistCount = Crud<Playlist>.GetBy("user", user.Id).Count;
                deleteModel.DownloadCount = Crud<Download>.GetBy("user", user.Id).Count;
            }

            return View(deleteModel);
        }

        // POST: AdminUsers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null || user.Email == "dilanalbacura887@gmail.com")
                {
                    TempData["Error"] = "Usuario no encontrado o no se puede eliminar";
                    return RedirectToAction("Index");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault();

                // Eliminar contenido relacionado según el rol
                if (userRole == "artists")
                {
                    // Eliminar músicas y álbumes del artista
                    var artistMusics = Crud<Musica>.GetBy("artist", user.Id);
                    var artistAlbums = Crud<Album>.GetBy("artist", user.Id);

                    foreach (var music in artistMusics)
                    {
                        Crud<Musica>.Delete(music.Id);
                    }

                    foreach (var album in artistAlbums)
                    {
                        Crud<Album>.Delete(album.Id);
                    }
                }
                else
                {
                    // Eliminar playlists y descargas del usuario
                    var userPlaylists = Crud<Playlist>.GetBy("user", user.Id);
                    var userDownloads = Crud<Download>.GetBy("user", user.Id);

                    foreach (var playlist in userPlaylists)
                    {
                        Crud<Playlist>.Delete(playlist.Id);
                    }

                    foreach (var download in userDownloads)
                    {
                        Crud<Download>.Delete(download.Id);
                    }
                }

                // Eliminar follows, favoritos, suscripciones, etc.
                CleanupUserRelatedData(user.Id);

                // Eliminar usuario
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["Success"] = $"Usuario {user.FirstName} {user.LastName} eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = "Error al eliminar usuario: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar usuario: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Métodos helper privados
        private async Task<List<UserListViewModel>> GetAllUsersWithRoles()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    RegistrationDate = user.RegistrationDate,
                    Role = roles.FirstOrDefault() ?? "users"
                });
            }

            return userList;
        }

        private UserSubscription GetActiveSubscription(int userId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync($"https://localhost:7095/api/UserSubscriptions/user/{userId}/active").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<UserSubscription>(json);
                    }
                }
            }
            catch { }
            return null;
        }

        private void CleanupUserRelatedData(int userId)
        {
            try
            {
                // Limpiar follows
                var userFollows = Crud<Follow>.GetBy("following", userId);
                var userFollowers = Crud<Follow>.GetBy("followers", userId);

                foreach (var follow in userFollows.Concat(userFollowers))
                {
                    Crud<Follow>.Delete(follow.Id);
                }

                // Limpiar favoritos
                var userFavoriteMusics = Crud<FavoritoMusica>.GetBy("user", userId);
                var userFavoriteArtists = Crud<FavoritoArtista>.GetBy("user", userId);

                foreach (var fav in userFavoriteMusics)
                {
                    using (var client = new HttpClient())
                    {
                        client.DeleteAsync($"https://localhost:7095/api/FavoriteMusics/user/{userId}/music/{fav.MusicId}").Wait();
                    }
                }

                foreach (var fav in userFavoriteArtists)
                {
                    using (var client = new HttpClient())
                    {
                        client.DeleteAsync($"https://localhost:7095/api/FavoriteArtists/user/{userId}/artist/{fav.ArtistId}").Wait();
                    }
                }

                // Limpiar suscripciones
                var userSubscriptions = Crud<UserSubscription>.GetBy("user", userId);
                foreach (var sub in userSubscriptions)
                {
                    Crud<UserSubscription>.Delete(sub.Id);
                }

                // Limpiar notificaciones
                var userNotifications = Crud<Notificacion>.GetBy("user", userId);
                foreach (var notification in userNotifications)
                {
                    Crud<Notificacion>.Delete(notification.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error limpiando datos relacionados: {ex.Message}");
            }
        }
    }
}
