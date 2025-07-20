using Aplicacion.API.Consumer;
using Aplicacion.Models;
using Aplicacion.Models.Identity;
using Aplicacion.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TagLib;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aplicacion.MVC.Controllers
{

    
    public class MusicsController : Controller
    {
        private readonly INotificationService _notificationService;
        public MusicsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        // GET: MusicsController
        [Authorize(Roles = "admins,artists")]
        public ActionResult Index()
        {
            if (User.IsInRole("artists"))
            {
                // Artista: solo sus músicas
                var currentUserId = GetCurrentUserId();
                var data = Crud<Music>.GetBy("artist", currentUserId);
                ViewBag.IsPersonalDashboard = true;
                return View(data);
            }
            else
            {
                // Admin: todas las músicas
                var data = Crud<Music>.GetAll();
                ViewBag.IsPersonalDashboard = false;
                return View(data);
            }
        }

        // GET: MusicsController/Details/5
        public ActionResult Details(int id)
        { 
            var data = Crud<Music>.GetById(id);
            return View(data);
        }

        // GET: MusicsController/Create
        [Authorize(Roles = "artists")]
        public ActionResult Create()
        {
            ViewBag.Genres = GetGenresList();
            ViewBag.Albums = GetAlbumsList();
            return View();
        }

        private List<SelectListItem> GetAlbumsList()
        {
            var currentUserId = GetCurrentUserId();
            var albums = Crud<Album>.GetBy("artist", currentUserId);

            var albumList = albums.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();

            // Agregar opción "Sin Album"
            albumList.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Sin Album --"
            });

            return albumList;
        }
        private List<SelectListItem> GetGenresList()
        {
            return Enum.GetValues(typeof(Music.MusicalGenre))
                .Cast<Music.MusicalGenre>()
                .Select(g => new SelectListItem
                {
                    Value = ((int)g).ToString(),
                    Text = g.ToString()
                }).ToList();
        }

        // POST: MusicsController/Create
        [Authorize(Roles = "artists")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]

        public ActionResult Create(Music data, IFormFile MusicFile, int? AlbumId)
        {
            try
            {
                // Validar archivo
                if (MusicFile == null || MusicFile.Length == 0)
                {
                    ModelState.AddModelError("", "Debe seleccionar un archivo de música");
                    ViewBag.Genres = GetGenresList();
                    ViewBag.Albums = GetAlbumsList();
                    return View(data);
                }

                // Asignar campos automáticos
                data.UploadDate = DateTime.Now;
                data.ArtistId = GetCurrentUserId();                
                data.FilePath = SaveMusicFile(MusicFile, data.ArtistId);
                data.Duration = GetAudioDuration(data.FilePath) ?? data.Duration ?? "";


                if (AlbumId.HasValue && AlbumId.Value > 0)
                {
                    data.AlbumId = AlbumId.Value;
                }

                Crud<Music>.Create(data);
                try
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.NotifyFollowersAsync(data.ArtistId, data.Title, data.Id);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error notificando seguidores: {ex.Message}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Genres = GetGenresList();
                ViewBag.Albums = GetAlbumsList();
                return View(data);
            }
        }


        // GET: MusicsController/Edit/5
        [Authorize(Roles = "artists")]
        public ActionResult Edit(int id)
        {
            var data = Crud<Music>.GetById(id);
            ViewBag.Genres = GetGenresList();
            ViewBag.Albums = GetAlbumsList();
            return View(data);
        }

        // POST: MusicsController/Edit/5
        [Authorize(Roles = "artists")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        public ActionResult Edit(int id, Music data, IFormFile MusicFile, int? AlbumId)
        {
            try
            {
                
                if (MusicFile != null && MusicFile.Length > 0)
                {
                    
                    var currentMusic = Crud<Music>.GetById(id);

                    if (currentMusic != null && !string.IsNullOrEmpty(currentMusic.FilePath)) // Delete if exist
                    {
                        DeleteMusicFile(currentMusic.FilePath);
                    }

                    
                    data.FilePath = SaveMusicFile(MusicFile, data.ArtistId); // Save
                    data.Duration = GetAudioDuration(data.FilePath) ?? data.Duration ?? "";
                }

                if (AlbumId.HasValue && AlbumId.Value > 0)
                {
                    data.AlbumId = AlbumId.Value;
                }
                else
                {
                    data.AlbumId = null; // Sin album
                }

                Crud<Music>.Update(id, data);
                TempData["Success"] = "Música actualizada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Genres = GetGenresList();
                ViewBag.Albums = GetAlbumsList();
                return View(data);
            }
        }

        private void DeleteMusicFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", filePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine($"Error eliminando archivo: {ex.Message}");
            }
        }

        // GET: MusicsController/Delete/5
        [Authorize(Roles = "admins,artists")]
        public ActionResult Delete(int id)
        {
            var data = Crud<Music>.GetById(id);
            return View(data);
        }

        // POST: MusicsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admins,artists")]
        public ActionResult Delete(int id, Music data)
        {
            try
            {
                Crud<Music>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        private int GetCurrentUserId()
        {
            if (User.Identity!.IsAuthenticated)
            {
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var userEmail = User.Identity.Name;
                var user = userManager.FindByNameAsync(userEmail).Result;
                return user?.Id ?? 0;
            }
            return 0;
        }

        private string SaveMusicFile(IFormFile file, int artistId)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "music", $"user_{artistId}");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return $"music/user_{artistId}/{fileName}";
        }

        
        private string GetAudioDuration(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", filePath);
                var file = TagLib.File.Create(fullPath);
                var duration = file.Properties.Duration;
                return $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}";
            }
            catch
            {
                return "3:00";
            }
        }


        [Authorize(Roles = "admins,artists,users")]
        public ActionResult Search(string query)
        {
            // All Musics
            if (string.IsNullOrEmpty(query))
            {
                var allMusics = Crud<Music>.GetAll();
                ViewBag.SearchQuery = "";
                ViewBag.ResultCount = allMusics.Count;
                ViewBag.ShowingAll = true; // Para cambiar el mensaje
                return View("SearchResults", allMusics);
            }

            var allMusicsFiltered = Crud<Music>.GetAll();

            // Fuzzy search 
            var filteredMusics = allMusicsFiltered.Where(m =>
                m.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (m.Artist?.FirstName + " " + m.Artist?.LastName).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                m.Artist?.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                m.Artist?.LastName.Contains(query, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();

            ViewBag.SearchQuery = query;
            ViewBag.ResultCount = filteredMusics.Count;
            ViewBag.ShowingAll = false;

            return View("SearchResults", filteredMusics);
        }

    }
}
