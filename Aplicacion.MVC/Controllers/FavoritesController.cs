using Aplicacion.API.Consumer;
using Aplicacion.Modelos;
using Aplicacion.Modelos.Favorite;
using Aplicacion.Modelos.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.MVC.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        // GET: Favorites - Vista principal con músicas favoritas
        public ActionResult Index()
        {
            var currentUserId = GetCurrentUserId();

            // Obtener músicas favoritas
            var favoriteMusics = GetFavoriteMusicsFromAPI(currentUserId);

            // Obtener artistas favoritos para el contador en tabs
            var favoriteArtists = GetFavoriteArtistsFromAPI(currentUserId);

            ViewBag.FavoriteMusics = favoriteMusics;
            ViewBag.FavoriteArtists = favoriteArtists;
            ViewBag.CurrentSection = "musics";

            return View();
        }

        // GET: Favorites/Artists - Vista de artistas favoritos
        public ActionResult Artists()
        {
            var currentUserId = GetCurrentUserId();

            var favoriteArtists = GetFavoriteArtistsFromAPI(currentUserId);
            var favoriteMusics = GetFavoriteMusicsFromAPI(currentUserId);

            ViewBag.FavoriteArtists = favoriteArtists;
            ViewBag.FavoriteMusics = favoriteMusics;
            ViewBag.CurrentSection = "artists";

            return View("Index"); // Usa la misma vista con diferente sección
        }

        // POST: Favorites/AddMusic/5
        [HttpPost]
        public ActionResult AddMusic(int musicId, string returnUrl = null)
        {
            try
            {
                var favorite = new FavoritoMusica
                {
                    UserId = GetCurrentUserId(),
                    MusicId = musicId,
                    AddedDate = DateTime.Now
                };

                Crud<FavoritoMusica>.Create(favorite);
                TempData["Success"] = "Música agregada a favoritos";
                return RedirectToReturnUrl(returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Warning"] = "Esta música ya está en tus favoritos";
                return RedirectToReturnUrl(returnUrl);
            }
        }

        // POST: Favorites/RemoveMusic/5
        [HttpPost]
        public ActionResult RemoveMusic(int musicId, string returnUrl = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Obtener favoritos del usuario para encontrar el ID correcto
                var userFavorites = Crud<FavoritoMusica>.GetBy("user", currentUserId);
                var favoriteToRemove = userFavorites.FirstOrDefault(fm => fm.MusicId == musicId);

                if (favoriteToRemove != null)
                {
                    // Usar endpoint específico para eliminar
                    using (var client = new HttpClient())
                    {
                        var response = client.DeleteAsync(
                            $"https://localhost:7095/api/FavoriteMusics/user/{currentUserId}/music/{musicId}").Result;

                        if (response.IsSuccessStatusCode)
                        {
                            TempData["Success"] = "Música eliminada de favoritos";
                        }
                        else
                        {
                            TempData["Error"] = "Error al eliminar de favoritos";
                        }
                    }
                }
                else
                {
                    TempData["Warning"] = "Esta música no está en tus favoritos";
                }

                return RedirectToReturnUrl(returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToReturnUrl(returnUrl);
            }
        }

        // POST: Favorites/AddArtist/5
        [HttpPost]
        public ActionResult AddArtist(int artistId, string returnUrl = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Verificar que no se agregue a sí mismo
                if (artistId == currentUserId)
                {
                    TempData["Error"] = "No puedes agregarte a ti mismo como favorito";
                    return RedirectToReturnUrl(returnUrl);
                }

                var favorite = new FavoritoArtista
                {
                    UserId = currentUserId,
                    ArtistId = artistId,
                    AddedDate = DateTime.Now
                };

                Crud<FavoritoArtista>.Create(favorite);
                TempData["Success"] = "Artista agregado a favoritos";
                return RedirectToReturnUrl(returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Warning"] = "Este artista ya está en tus favoritos";
                return RedirectToReturnUrl(returnUrl);
            }
        }

        // POST: Favorites/RemoveArtist/5
        [HttpPost]
        public ActionResult RemoveArtist(int artistId, string returnUrl = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Obtener favoritos del usuario para encontrar el ID correcto
                var userFavorites = Crud<FavoritoArtista>.GetBy("user", currentUserId);
                var favoriteToRemove = userFavorites.FirstOrDefault(fa => fa.ArtistId == artistId);

                if (favoriteToRemove != null)
                {
                    // Usar endpoint específico para eliminar
                    using (var client = new HttpClient())
                    {
                        var response = client.DeleteAsync(
                            $"https://localhost:7095/api/FavoriteArtists/user/{currentUserId}/artist/{artistId}").Result;

                        if (response.IsSuccessStatusCode)
                        {
                            TempData["Success"] = "Artista eliminado de favoritos";
                        }
                        else
                        {
                            TempData["Error"] = "Error al eliminar artista";
                        }
                    }
                }
                else
                {
                    TempData["Warning"] = "Este artista no está en tus favoritos";
                }

                return RedirectToReturnUrl(returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToReturnUrl(returnUrl);
            }
        }

        // GET: Favorites/CheckMusicFavorite/5 - AJAX para verificar si está en favoritos
        [HttpGet]
        public JsonResult CheckMusicFavorite(int musicId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userFavorites = Crud<FavoritoMusica>.GetBy("user", currentUserId);
                var isFavorite = userFavorites.Any(fm => fm.MusicId == musicId);

                return Json(new { isFavorite = isFavorite });
            }
            catch
            {
                return Json(new { isFavorite = false });
            }
        }

        // GET: Favorites/CheckArtistFavorite/5 - AJAX para verificar si está en favoritos
        [HttpGet]
        public JsonResult CheckArtistFavorite(int artistId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userFavorites = Crud<FavoritoArtista>.GetBy("user", currentUserId);
                var isFavorite = userFavorites.Any(fa => fa.ArtistId == artistId);

                return Json(new { isFavorite = false });
            }
            catch
            {
                return Json(new { isFavorite = false });
            }
        }

        // Método privado para obtener músicas favoritas
        private List<Musica> GetFavoriteMusicsFromAPI(int userId)
        {
            try
            {
                var favoriteMusics = Crud<FavoritoMusica>.GetBy("user", userId);

                // Extraer las músicas de los favoritos
                var musicIds = favoriteMusics.Select(fm => fm.MusicId).ToList();
                var allMusics = Crud<Musica>.GetAll();

                return allMusics.Where(m => musicIds.Contains(m.Id)).ToList();
            }
            catch
            {
                return new List<Musica>();
            }
        }

        // Método privado para obtener artistas favoritos
        private List<User> GetFavoriteArtistsFromAPI(int userId)
        {
            try
            {
                var favoriteArtists = Crud<FavoritoArtista>.GetBy("user", userId);

                // Extraer los artistas de los favoritos
                var artistIds = favoriteArtists.Select(fa => fa.ArtistId).ToList();

                // Obtener artistas desde las músicas (ya que no tenemos endpoint directo de usuarios)
                var allMusics = Crud<Musica>.GetAll();
                var artists = allMusics.Select(m => m.Artist)
                    .Where(a => a != null && artistIds.Contains(a.Id))
                    .GroupBy(a => a.Id)
                    .Select(g => g.First())
                    .ToList();

                return artists;
            }
            catch
            {
                return new List<User>();
            }
        }





        // GET: Favorites/DeleteMusic/5
        public ActionResult DeleteMusic(int musicId, string returnUrl = null)
        {
            try
            {
                var music = Crud<Musica>.GetById(musicId);
                if (music == null)
                {
                    TempData["Error"] = "Música no encontrada";
                    return RedirectToAction("Index");
                }

                ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index");
                return View(music);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Favorites/ConfirmRemoveMusic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmRemoveMusic(int musicId, string returnUrl = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                using (var client = new HttpClient())
                {
                    var response = client.DeleteAsync(
                        $"https://localhost:7095/api/FavoriteMusics/user/{currentUserId}/music/{musicId}").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Música eliminada de favoritos exitosamente";
                    }
                    else
                    {
                        TempData["Error"] = "Error al eliminar de favoritos";
                    }
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Favorites/DeleteArtist/5
        public ActionResult DeleteArtist(int artistId, string returnUrl = null)
        {
            try
            {
                // Obtener artista desde las músicas
                var allMusics = Crud<Musica>.GetAll();
                var artist = allMusics.Select(m => m.Artist)
                    .FirstOrDefault(a => a != null && a.Id == artistId);

                if (artist == null)
                {
                    TempData["Error"] = "Artista no encontrado";
                    return RedirectToAction("Artists");
                }

                ViewBag.ReturnUrl = returnUrl ?? Url.Action("Artists");
                return View(artist);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("Artists");
            }
        }

        // POST: Favorites/ConfirmRemoveArtist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmRemoveArtist(int artistId, string returnUrl = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                using (var client = new HttpClient())
                {
                    var response = client.DeleteAsync(
                        $"https://localhost:7095/api/FavoriteArtists/user/{currentUserId}/artist/{artistId}").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Artista eliminado de favoritos exitosamente";
                    }
                    else
                    {
                        TempData["Error"] = "Error al eliminar artista";
                    }
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Artists");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("Artists");
            }
        }



        private ActionResult RedirectToReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // Método privado para obtener ID del usuario actual
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
