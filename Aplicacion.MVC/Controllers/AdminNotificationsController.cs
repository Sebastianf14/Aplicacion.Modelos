using Aplicacion.API.Consumer;
using Aplicacion.Models.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "admins")]
    public class AdminNotificationsController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                var allNotifications = Crud<Notification>.GetAll().OrderByDescending(n => n.CreatedDate).ToList();

                ViewBag.Stats = new
                {
                    Total = allNotifications.Count,
                    Unread = allNotifications.Count(n => !n.IsRead),
                    NewMusic = allNotifications.Count(n => n.Type == Notification.NotificationType.NewMusic),
                    Welcome = allNotifications.Count(n => n.Type == Notification.NotificationType.Welcome),
                    System = allNotifications.Count(n => n.Type == Notification.NotificationType.System),
                    Today = allNotifications.Count(n => n.CreatedDate.Date == DateTime.Today),
                    ThisWeek = allNotifications.Count(n => n.CreatedDate >= DateTime.Now.AddDays(-7))
                };

                return View(allNotifications);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error cargando notificaciones: " + ex.Message;
                return View(new List<Notification>());
            }
        }

        // GET: AdminNotifications/ByType/NewMusic
        public ActionResult ByType(string type)
        {
            try
            {
                var allNotifications = Crud<Notification>.GetAll();

                if (Enum.TryParse<Notification.NotificationType>(type, out var notificationType))
                {
                    var filteredNotifications = allNotifications
                        .Where(n => n.Type == notificationType)
                        .OrderByDescending(n => n.CreatedDate)
                        .ToList();

                    ViewBag.FilterType = type;
                    return View("Index", filteredNotifications);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error filtrando notificaciones: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
