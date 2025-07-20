using Aplicacion.API.Consumer;
using Aplicacion.Models.Identity;
using Aplicacion.Models.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "admins,users")]
    public class DownloadsController : Controller
    {
        // GET: DownloadsController
        public ActionResult Index()
        {
            var currentUserId = GetCurrentUserId();
            var data = Crud<Download>.GetBy("user", currentUserId);
            return View(data);
        }

        // GET: DownloadsController/Details/5
        public ActionResult Details(int id)
        {
            var data = Crud<Download>.GetById(id);

            if (data.UserId != GetCurrentUserId() && !User.IsInRole("admins"))
            {
                return Forbid();
            }
            return View(data);
        }

        // GET: DownloadsController/Create
        public ActionResult Create()
        {

            return View();
        }

        // POST: DownloadsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Download data)
        {
            try
            {
                data.UserId = GetCurrentUserId(); 
                Crud<Download>.Create(data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: DownloadsController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<Download>.GetById(id);
            return View(data);
        }

        // POST: DownloadsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Download data)
        {
            try
            {
                Crud<Download>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: DownloadsController/Delete/5
        public ActionResult Delete(int id)
        {
            var data = Crud<Download>.GetById(id);
            if (data.UserId != GetCurrentUserId() && !User.IsInRole("admins"))
            {
                return Forbid();
            }
            return View(data);
        }

        // POST: DownloadsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Download data)
        {
            try
            {
                Crud<Download>.Delete(id);
                TempData["Success"] = "Descarga eliminada del historial";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(data);
            }

        }

        [HttpPost]
        public ActionResult DownloadMusic(int musicId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Verificar si ya descargó esta música
                var existingDownloads = Crud<Download>.GetBy("user", currentUserId);
                if (existingDownloads.Any(d => d.MusicId == musicId))
                {
                    TempData["Warning"] = "Ya has descargado esta canción anteriormente";
                    return RedirectToAction("Search", "Musics");
                }

                // Crear registro de descarga
                var download = new Download
                {
                    UserId = currentUserId,
                    MusicId = musicId,
                    DownloadDate = DateTime.Now,
                    Status = Download.DownloadStatus.Completed
                };

                Crud<Download>.Create(download);
                TempData["Success"] = "Música descargada exitosamente";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al descargar: " + ex.Message;
                return RedirectToAction("Search", "Musics");
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
