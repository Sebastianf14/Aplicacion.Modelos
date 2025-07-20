using Aplicacion.API.Consumer;
using Aplicacion.Models;
using Aplicacion.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.MVC.Controllers
{
    [Authorize]
    public class PlaylistMusicController : Controller
    {

        [HttpPost]
        public async Task<IActionResult> AddToPlaylist(int playlistId, int musicId)
        {
            try
            {
                // Verificar que la playlist pertenece al usuario actual
                var playlist = Crud<Playlist>.GetById(playlistId);
                var currentUserId = GetCurrentUserId();

                if (playlist.UserId != currentUserId && !User.IsInRole("admins"))
                {
                    return Forbid();
                }

                // Llamar al endpoint de la API
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(
                        $"https://localhost:7095/api/Playlists/{playlistId}/music/{musicId}",
                        null);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Música agregada a la playlist exitosamente";
                    }
                    else
                    {
                        TempData["Error"] = "Error al agregar música a la playlist";
                    }
                }

                return RedirectToAction("Search", "Musics");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Search", "Musics");
            }

        }


        [HttpPost]
        public async Task<IActionResult> RemoveFromPlaylist(int playlistId, int musicId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.DeleteAsync(
                        $"https://localhost:7095/api/Playlists/{playlistId}/music/{musicId}");

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Música removida de la playlist";
                    }
                    else
                    {
                        TempData["Error"] = "Error al remover música";
                    }
                }

                return RedirectToAction("Details", "Playlists", new { id = playlistId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Playlists", new { id = playlistId });
            }
        }

        private int GetCurrentUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var userEmail = User.Identity.Name;
                var user = userManager.FindByNameAsync(userEmail).Result;
                return user?.Id ?? 0;
            }
            return 0;
        }

    }
}
