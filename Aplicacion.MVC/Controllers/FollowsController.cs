using Aplicacion.API.Consumer;
using Aplicacion.Models;
using Aplicacion.Models.Identity;
using Aplicacion.Models.Implementations;
using Aplicacion.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "users,artists")]
    public class FollowsController : Controller
    {
        private readonly INotificationService _notificationService;
        public FollowsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: FollowController
        public ActionResult Index()
        {
                // Usuario: solo artistas que sigue
                var currentUserId = GetCurrentUserId();
                var data = Crud<Follow>.GetBy("following", currentUserId);
                ViewBag.CurrentSection = "following";
                return View(data);
        }
        public ActionResult Followers()
        {
                // Usuario: sus seguidores
                var currentUserId = GetCurrentUserId();
                var data = Crud<Follow>.GetBy("followers", currentUserId);
                ViewBag.CurrentSection = "followers";
                return View("Index", data);
            
        }

        // GET: Follow/Discover - Descubrir 
        public ActionResult Discover()
        {
                // Usuario: artistas que no sigue
                var allMusics = Crud<Music>.GetAll();
                var artists = allMusics.Select(m => m.Artist)
                    .Where(a => a != null)
                    .GroupBy(a => a.Id)  // Agrupar por ID
                    .Select(g => g.First()) // Tomar el primero de cada grupo
                    .ToList();

                var currentUserId = GetCurrentUserId();
                var following = Crud<Follow>.GetBy("following", currentUserId);
                var followingIds = following.Select(f => f.ArtistId).ToList();

                var availableArtists = artists.Where(a => !followingIds.Contains(a.Id) && a.Id != currentUserId).ToList();

                ViewBag.CurrentSection = "discover";
                return View(availableArtists);
            
        }


        // POST: Follow/FollowArtist/5
        [HttpPost]
        public ActionResult FollowArtist(int artistId, string returnUrl = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Verificar que no se siga a sí mismo
                if (artistId == currentUserId)
                {
                    TempData["Error"] = "No puedes seguirte a ti mismo";
                    return RedirectToReturnUrl(returnUrl);
                }

                // Verificar si ya lo sigue
                var existing = Crud<Follow>.GetBy("following", currentUserId);
                if (existing.Any(f => f.ArtistId == artistId))
                {
                    TempData["Warning"] = "Ya sigues a este artista";
                    return RedirectToReturnUrl(returnUrl);
                }

                var follow = new Follow
                {
                    FollowerId = currentUserId,
                    ArtistId = artistId,
                    FollowDate = DateTime.Now
                };

                Crud<Follow>.Create(follow);

                try
                {
                    var currentUser = GetCurrentUser();
                    var followerName = $"{currentUser?.FirstName} {currentUser?.LastName}".Trim();

                    _ = Task.Run(async () =>
                    {
                        await _notificationService.NotifyArtistNewFollowerAsync(artistId, followerName);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error notificando artista: {ex.Message}");
                }

                return RedirectToReturnUrl(returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al seguir artista: " + ex.Message;
                return RedirectToReturnUrl(returnUrl);
            }
        }
        // POST: Follow/UnfollowArtist/5
        [HttpPost]
        public ActionResult UnfollowArtist(int artistId, string returnUrl = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var following = Crud<Follow>.GetBy("following", currentUserId);
                var followToRemove = following.FirstOrDefault(f => f.ArtistId == artistId);

                if (followToRemove != null)
                {
                    Crud<Follow>.Delete(followToRemove.Id);
                    TempData["Success"] = "Has dejado de seguir a este artista";
                }
                else
                {
                    TempData["Warning"] = "No sigues a este artista";
                }

                return RedirectToReturnUrl(returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al dejar de seguir: " + ex.Message;
                return RedirectToReturnUrl(returnUrl);
            }
        }


        // GET: FollowController/Details/5
        public ActionResult Details(int id)
        {
            var data = Crud<Follow>.GetById(id);
            return View(data);
        }

        // GET: FollowController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FollowController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Follow data)
        {
            try
            {
                data.FollowerId = GetCurrentUserId(); 
                data.FollowDate = DateTime.Now;

                Crud<Follow>.Create(data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: FollowController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<Follow>.GetById(id);
            return View(data);
        }

        // POST: FollowController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Follow data)
        {
            try
            {
                Crud<Follow>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: FollowController/Delete/5
        public ActionResult Delete(int id)
        {
            var follow = Crud<Follow>.GetById(id);

            if (follow.FollowerId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(follow);    
        }

        // POST: FollowController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Follow data)
        {
            try
            {
                Crud<Follow>.Delete(id);
                TempData["Success"] = "Has dejado de seguir a este artista";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
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
        private ActionResult RedirectToReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Search", "Musics");
        }
        private User GetCurrentUser()
        {
            if (User.Identity!.IsAuthenticated)
            {
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var userEmail = User.Identity.Name;
                return userManager.FindByNameAsync(userEmail).Result;
            }
            return null;
        }
    }
}
