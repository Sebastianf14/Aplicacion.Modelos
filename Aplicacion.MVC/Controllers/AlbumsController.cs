using Aplicacion.API.Consumer;
using Aplicacion.Models;
using Aplicacion.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "admins,artists")]
    public class AlbumsController : Controller
    {
        // GET: AlbumsController
        public ActionResult Index()
        {
            if (User.IsInRole("artists"))
            {
                // Artista: solo sus albums
                var currentUserId = GetCurrentUserId();
                var data = Crud<Album>.GetBy("artist", currentUserId);
                return View(data);
            }
            else
            {
                // Admin: todos los albums
                var data = Crud<Album>.GetAll();
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

        // GET: AlbumsController/Details/5
        public ActionResult Details(int id)
        {
            var data = Crud<Album>.GetById(id);
            return View(data);
        }

        // GET: AlbumsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AlbumsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Album data)
        {
            try
            {
                data.ArtistId = GetCurrentUserId();
                Crud<Album>.Create(data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: AlbumsController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<Album>.GetById(id);
            return View(data);
        }

        // POST: AlbumsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Album data)
        {
            try
            {
                Crud<Album>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: AlbumsController/Delete/5
        public ActionResult Delete(int id)
        {
            var data = Crud<Album>.GetById(id);
            return View(data);
        }

        // POST: AlbumsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Album data)
        {
            try
            {
                Crud<Album>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }
    }
}
