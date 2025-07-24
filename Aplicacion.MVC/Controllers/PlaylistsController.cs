using Aplicacion.API.Consumer;
using Aplicacion.Modelos;
using Aplicacion.Modelos.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.Runtime.InteropServices;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "admins,users")]
    public class PlaylistsController : Controller
    {
        // GET: PlaylistsController
        public ActionResult Index()
        {
            if (User.IsInRole("admins"))
            {
                // Admin: todas las playlists
                var data = Crud<Playlist>.GetAll();
                return View(data);
            }
            else
            {
                // Usuario: solo sus playlists
                var currentUserId = GetCurrentUserId();
                var data = Crud<Playlist>.GetBy("user", currentUserId);
                return View(data);
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
        private Playlist GetPlaylistWithMusics(int playlistId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync($"https://localhost:7095/api/Playlists/{playlistId}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        var playlist = Newtonsoft.Json.JsonConvert.DeserializeObject<Playlist>(json);
                        return playlist;
                    }
                    else
                    {
                        
                        return Crud<Playlist>.GetById(playlistId);
                    }
                }
            }
            catch (Exception)
            {
                
                return Crud<Playlist>.GetById(playlistId);
            }
        }
        // GET: PlaylistsController/Details/5
        public ActionResult Details(int id)
        {
            var data = GetPlaylistWithMusics(id);
            if (data.PlaylistMusics?.Any() == true)
            {
                var playlistTracks = data.PlaylistMusics.OrderBy(pm => pm.Order).Select(pm => new
                {
                    id = pm.Music.Id,
                    titulo = pm.Music.Title,
                    artista = $"{pm.Music.Artist?.FirstName} {pm.Music.Artist?.LastName}".Trim(),
                    audioUrl = $"/files/{pm.Music.FilePath}"
                }).ToList();

                ViewBag.PlaylistTracksJson = System.Text.Json.JsonSerializer.Serialize(playlistTracks);
            }
            else
            {
                ViewBag.PlaylistTracksJson = "[]";
            }

            return View(data);
        }

        // GET: PlaylistsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PlaylistsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Playlist data)
        {
            try
            {
                data.UserId = GetCurrentUserId();
                Crud<Playlist>.Create(data);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: PlaylistsController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<Playlist>.GetById(id);
            return View(data);
        }

        // POST: PlaylistsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Playlist data)
        {
            try
            {
                Crud<Playlist>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: PlaylistsController/Delete/5
        public ActionResult Delete(int id)
        {
            var data = Crud<Playlist>.GetById(id);
            return View(data);
        }

        // POST: PlaylistsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Playlist data)
        {
            try
            {
                Crud<Playlist>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: /Playlists/GetUserPlaylists (para AJAX)
        [HttpGet]
        public JsonResult GetUserPlaylists()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var playlists = Crud<Playlist>.GetBy("user", currentUserId);

                var playlistsData = playlists.Select(p => new {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    isPublic = p.IsPublic,
                    songsCount = p.PlaylistMusics?.Count ?? 0
                });

                return Json(playlistsData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }




    }
}
