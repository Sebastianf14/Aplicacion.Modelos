using Aplicacion.API.Consumer;
using Aplicacion.Modelos.Identity;
using Aplicacion.Modelos.Implementations;
using Aplicacion.Modelos.Suscription;
using Aplicacion.MVC.Models;
using Aplicacion.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "admins,users")]
    public class UserSubscriptionsController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;


        public UserSubscriptionsController(IEmailService emailService, INotificationService notificationService)
        {
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // GET: UserSubscriptionsController
        public ActionResult Index()
        {
            if (User.IsInRole("admins"))
            {
                var allSubscriptions = Crud<UserSubscription>.GetAll();
                var allNotifications = Crud<Notificacion>.GetAll();

                ViewBag.IsAdmin = true;
                ViewBag.AllNotifications = allNotifications.OrderByDescending(n => n.CreatedDate).ToList();
                ViewBag.NotificationStats = GetNotificationStats(allNotifications);

                return View(allSubscriptions);
            }
            else
            {
                var currentUserId = GetCurrentUserId();
                var userSubscriptions = Crud<UserSubscription>.GetBy("user", currentUserId);
                ViewBag.IsAdmin = false;
                ViewBag.CurrentSubscription = GetActiveSubscription(currentUserId);
                return View(userSubscriptions);
            }
        }
        private object GetNotificationStats(List<Notificacion> notifications)
        {
            return new
            {
                Total = notifications.Count,
                Unread = notifications.Count(n => !n.IsRead),
                NewMusic = notifications.Count(n => n.Type == Notificacion.NotificationType.NewMusic),
                Welcome = notifications.Count(n => n.Type == Notificacion.NotificationType.Welcome),
                System = notifications.Count(n => n.Type == Notificacion.NotificationType.System),
                SubscriptionExpiry = notifications.Count(n => n.Type == Notificacion.NotificationType.SubscriptionExpiry),
                Today = notifications.Count(n => n.CreatedDate.Date == DateTime.Today),
                ThisWeek = notifications.Count(n => n.CreatedDate >= DateTime.Now.AddDays(-7))
            };
        }

        // GET: UserSubscriptionsController/Details/5
        public ActionResult Details(int id)
        {
            var data = Crud<UserSubscription>.GetById(id);
            return View(data);
        }

        // GET: UserSubscriptionsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserSubscriptionsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public ActionResult Create(UserSubscription data)
        {
            try
            {
                data.UserId = GetCurrentUserId(); // Asignar usuario actual
                data.StartDate = DateTime.Now;
                data.IsActive = true;
                data.Status = UserSubscription.SubscriptionStatus.Active;

                Crud<UserSubscription>.Create(data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: UserSubscriptionsController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<UserSubscription>.GetById(id);
            return View(data);
        }

        // POST: UserSubscriptionsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UserSubscription data)
        {
            try
            {
                Crud<UserSubscription>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: UserSubscriptionsController/Delete/5
        public ActionResult Delete(int id)
        {
            var data = Crud<UserSubscription>.GetById(id);
            return View(data);
        }

        // POST: UserSubscriptionsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, UserSubscription data)
        {
            try
            {
                Crud<UserSubscription>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }



        
        public ActionResult Plans()
        {
            var activePlans = GetActivePlans();
            var currentUserId = GetCurrentUserId();
            var currentSubscription = GetActiveSubscription(currentUserId);

            ViewBag.CurrentSubscription = currentSubscription;
            ViewBag.UserSubscriptions = Crud<UserSubscription>.GetBy("user", currentUserId);

            return View(activePlans);
        }

        // GET: UserSubscriptions/Payment - Formulario de pago
        public ActionResult Payment(int planId, int months = 1)
        {
            var plan = Crud<SubscriptionPlan>.GetById(planId);
            if (plan == null || !plan.IsActive || plan.Price == 0)
            {
                TempData["Error"] = "Plan no válido para pago";
                return RedirectToAction("Plans");
            }

            var currentUser = GetCurrentUser();
            var model = new PaymentViewModel
            {
                PlanId = planId,
                Months = months,
                TotalAmount = plan.Price * months,
                PlanName = plan.Name,
                Email = currentUser?.Email ?? ""
            };

            return View(model);
        }

        // POST: UserSubscriptions/ProcessPayment - Procesar pago simulado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Payment", model);
            }

            try
            {
                var plan = Crud<SubscriptionPlan>.GetById(model.PlanId);
                var currentUserId = GetCurrentUserId();
                var currentUser = GetCurrentUser();

                // Verificar si ya tiene este plan
                var currentSubscription = GetActiveSubscription(currentUserId);
                if (currentSubscription != null && currentSubscription.SubscriptionPlanId == model.PlanId)
                {
                    TempData["Warning"] = "Ya tienes este plan activo. No puedes comprar el mismo plan nuevamente.";
                    return RedirectToAction("Plans");
                }

                // Simular validación de tarjeta
                if (!ValidateCard(model))
                {
                    TempData["Error"] = "Error en el procesamiento del pago. Verifique los datos de la tarjeta.";
                    return View("Payment", model);
                }

                // Cancelar suscripción actual si no es gratuita
                if (currentSubscription != null && currentSubscription.SubscriptionPlan?.Price > 0)
                {
                    await CancelSubscriptionInternal(currentSubscription.Id);
                }

                // Crear nueva suscripción
                var newSubscription = new UserSubscription
                {
                    UserId = currentUserId,
                    SubscriptionPlanId = model.PlanId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(model.Months),
                    IsActive = true,
                    Status = UserSubscription.SubscriptionStatus.Active,
                    PaymentMethod = $"Tarjeta ****{model.CardNumber.Substring(12)}"
                };

                var createdSubscription = Crud<UserSubscription>.Create(newSubscription);

                // Enviar comprobante por email
                await SendPaymentConfirmation(model, plan, currentUser);


                
                try
                {
                    await _notificationService.NotifyUserSubscriptionAsync(currentUserId, plan.Name); // ✅ CAMBIAR
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error enviando notificación: {ex.Message}");
                }

                TempData["Success"] = $"¡Pago procesado exitosamente! Ahora tienes el plan {plan.Name}";
                return RedirectToAction("PaymentSuccess");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error procesando el pago: " + ex.Message;
                return View("Payment", model);
            }
        }

        // GET: UserSubscriptions/PaymentSuccess - Página de confirmación
        public ActionResult PaymentSuccess()
        {
            var currentUserId = GetCurrentUserId();
            var subscription = GetActiveSubscription(currentUserId);

            if (subscription == null)
            {
                TempData["Error"] = "No se encontró suscripción activa";
                return RedirectToAction("Index");
            }

            return View(subscription);
        }

        // POST: UserSubscriptions/ChangePlan - Cambiar entre planes pagos

        [HttpPost]
        public ActionResult ChangePlan(int newPlanId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentSubscription = GetActiveSubscription(currentUserId);
                var newPlan = Crud<SubscriptionPlan>.GetById(newPlanId);

                if (currentSubscription?.SubscriptionPlanId == newPlanId)
                {
                    TempData["Warning"] = "Ya tienes este plan activo";
                    return RedirectToAction("Plans");
                }

                if (newPlan == null || !newPlan.IsActive)
                {
                    TempData["Error"] = "Plan no disponible";
                    return RedirectToAction("Plans");
                }

                // No permitir cambio a gratuito si tiene plan pagado
                if (newPlan.Price == 0 && currentSubscription?.SubscriptionPlan?.Price > 0)
                {
                    TempData["Error"] = "No puedes cambiar a plan gratuito desde un plan pagado";
                    return RedirectToAction("Plans");
                }

                // Si es gratuito a pagado, redirigir a pago
                if (newPlan.Price > 0)
                {
                    return RedirectToAction("Payment", new { planId = newPlanId, months = 1 });
                }

                // Cambio entre planes del mismo precio (futuro feature)
                TempData["Info"] = "Función de cambio de plan en desarrollo";
                return RedirectToAction("Plans");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("Plans");
            }
        }

        // POST: UserSubscriptions/Cancel - Cancelar suscripción
        [HttpPost]
        public async Task<ActionResult> Cancel(int subscriptionId)
        {
            try
            {
                var result = await CancelSubscriptionInternal(subscriptionId);
                if (result)
                {
                    TempData["Success"] = "Suscripción cancelada exitosamente";
                }
                else
                {
                    TempData["Error"] = "Error al cancelar suscripción";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }




        // Métodos privados
        private bool ValidateCard(PaymentViewModel model)
        {
            // Validar con algoritmo de Luhn
            if (!IsValidLuhn(model.CardNumber))
                return false;

            // Números de tarjeta de prueba que fallan intencionalmente
            var invalidTestCards = new[] { "4000000000000002", "4000000000000119" };
            if (invalidTestCards.Contains(model.CardNumber))
                return false;

            // Validar fecha de expiración
            var expiry = new DateTime(model.ExpiryYear, model.ExpiryMonth, 1);
            if (expiry <= DateTime.Now)
                return false;

            return true;
        }
        private bool IsValidLuhn(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 13 || cardNumber.Length > 19)
                return false;

            int sum = 0;
            bool alternate = false;

            // Procesar de derecha a izquierda
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(cardNumber[i]))
                    return false;

                int digit = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit = (digit % 10) + 1;
                }

                sum += digit;
                alternate = !alternate;
            }

            return (sum % 10) == 0;
        }

        private async Task SendPaymentConfirmation(PaymentViewModel model, SubscriptionPlan plan, User user)
        {
            try
            {
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #667eea;'>¡Gracias por tu suscripción a SonikoMusic!</h2>
                        
                        <div style='background: #f8f9fa; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                            <h3>Detalles de tu suscripción:</h3>
                            <p><strong>Plan:</strong> {plan.Name}</p>
                            <p><strong>Duración:</strong> {model.Months} mes(es)</p>
                            <p><strong>Total pagado:</strong> ${model.TotalAmount:F2}</p>
                            <p><strong>Método de pago:</strong> Tarjeta ****{model.CardNumber.Substring(12)}</p>
                            <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>

                        <div style='background: #e8f5e8; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='color: #28a745; margin-top: 0;'>Beneficios de tu plan {plan.Name}:</h4>
                            <ul>
                                {(plan.AllowDownloads ? "<li>✅ Descargas ilimitadas</li>" : "")}
                                {(plan.AllowOfflineMode ? "<li>✅ Modo offline</li>" : "")}
                                {(plan.MaxPlaylists == -1 ? "<li>✅ Playlists ilimitadas</li>" : $"<li>✅ Hasta {plan.MaxPlaylists} playlists</li>")}
                                {(!plan.HasAds ? "<li>✅ Sin publicidad</li>" : "")}
                            </ul>
                        </div>

                        <p>¡Disfruta de tu música sin límites!</p>
                        <p style='color: #666; font-size: 12px;'>
                            Este es un comprobante automático. Si tienes dudas, contacta nuestro soporte.
                        </p>
                    </div>
                ";

                await _emailService.SendEmailAsync(model.Email, "Comprobante de Pago - SonikoMusic", emailBody);
            }
            catch (Exception ex)
            {
                // Log error pero no interrumpir el proceso
                System.Diagnostics.Debug.WriteLine($"Error enviando email: {ex.Message}");
            }
        }

        private async Task<bool> CancelSubscriptionInternal(int subscriptionId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(
                        $"https://localhost:7095/api/UserSubscriptions/{subscriptionId}/cancel",
                        null);

                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
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

        private List<SubscriptionPlan> GetActivePlans()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync("https://localhost:7095/api/SubscriptionPlans/active").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<SubscriptionPlan>>(json) ?? new List<SubscriptionPlan>();
                    }
                }
            }
            catch { }

            return new List<SubscriptionPlan>();
        }

        private User GetCurrentUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var userEmail = User.Identity.Name;
                return userManager.FindByNameAsync(userEmail).Result;
            }
            return null;
        }

        private int GetCurrentUserId()
        {
            return GetCurrentUser()?.Id ?? 0;
        }


    }
}
