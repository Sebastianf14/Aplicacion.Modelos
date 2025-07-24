using Aplicacion.API.Consumer;
using Aplicacion.Modelos.Identity;
using Aplicacion.Modelos.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aplicacion.MVC.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        // GET: NotificationsController
        public ActionResult Index()
        {
            var currentUserId = GetCurrentUserId();
            var notifications = Crud<Notificacion>.GetBy("user", currentUserId);

            // Ordenar por fecha descendente
            var sortedNotifications = notifications.OrderByDescending(n => n.CreatedDate).ToList();

            return View(sortedNotifications);
        }

        // GET: NotificationsController/Details/5
        public ActionResult Details(int id)
        {
            var data = Crud<Notificacion>.GetById(id);
            return View(data);
        }

        // GET: NotificationsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: NotificationsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Notificacion data)
        {
            try
            {
                data.UserId = GetCurrentUserId(); 
                data.CreatedDate = DateTime.Now;
                data.IsRead = false;

                Crud<Notificacion>.Create(data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: NotificationsController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<Notificacion>.GetById(id);
            return View(data);
        }

        // POST: NotificationsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Notificacion data)
        {
            try
            {
                Crud<Notificacion>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: NotificationsController/Delete/5
        public ActionResult Delete(int id)
        {
            var data = Crud<Notificacion>.GetById(id);
            return View(data);
        }

        // POST: NotificationsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Notificacion data)
        {
            try
            {
                Crud<Notificacion>.Delete(id);
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
            if (User.Identity.IsAuthenticated)
            {
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var userEmail = User.Identity.Name;
                var user = userManager.FindByNameAsync(userEmail).Result;
                return user?.Id ?? 0;
            }
            return 0;
        }




        // GET: Notifications/Unread
        public ActionResult Unread()
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync($"https://localhost:7095/api/Notifications/user/{currentUserId}/unread").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        var notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Notificacion>>(json) ?? new List<Notificacion>();
                        return View("Index", notifications.OrderByDescending(n => n.CreatedDate).ToList());
                    }
                }
            }
            catch { }

            return View("Index", new List<Notificacion>());
        }


        // POST: Notifications/MarkAsRead/5
        [HttpPost]
        public ActionResult MarkAsRead(int id, string returnUrl = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.PutAsync($"https://localhost:7095/api/Notifications/{id}/markread", null).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Notificación marcada como leída";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // POST: Notifications/MarkAllAsRead
        [HttpPost]
        public ActionResult MarkAllAsRead()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                using (var client = new HttpClient())
                {
                    var response = client.PutAsync($"https://localhost:7095/api/Notifications/markallread/user/{currentUserId}", null).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Todas las notificaciones marcadas como leídas";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: Notifications/GetUnreadCount - Para AJAX
        [HttpGet]
        public JsonResult GetUnreadCount()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                using (var client = new HttpClient())
                {
                    var response = client.GetAsync($"https://localhost:7095/api/Notifications/user/{currentUserId}/count").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var countStr = response.Content.ReadAsStringAsync().Result;
                        var count = int.Parse(countStr);
                        return Json(new { count = count });
                    }
                }
            }
            catch { }

            return Json(new { count = 0 });
        }

        // GET: Notifications/GetRecent - Para dropdown en navbar
        [HttpGet]
        public JsonResult GetRecent()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var notifications = Crud<Notificacion>.GetBy("user", currentUserId);

                var recent = notifications
                    .OrderByDescending(n => n.CreatedDate)
                    .Take(3)
                    .Select(n => new {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        isRead = n.IsRead,
                        createdDate = n.CreatedDate.ToString("dd/MM HH:mm"),
                        actionUrl = n.ActionUrl
                    })
                    .ToList();

                return Json(recent);
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        







    }
}
