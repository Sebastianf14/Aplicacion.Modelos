using Aplicacion.Modelos.Identity;
using Aplicacion.Modelos.Suscription;
using Fluent.Infrastructure.FluentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NETCore.MailKit.Core;
using Umbraco.Core.Services;

namespace Aplicacion.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;


        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, IEmailService emailService, INotificationService notificationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _notificationService = notificationService; 

        }


        // GET: Account/Register
        public IActionResult Register()
        {
            ViewBag.Roles = GetRolesList();
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegistrationDate = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Asignar rol
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // ⭐ NUEVO: Asignar plan gratuito por defecto
                    try
                    {
                        var freePlan = Crud<SubscriptionPlan>.GetAll().FirstOrDefault(p => p.Price == 0 && p.Name.ToLower().Contains("gratuito"));
                        if (freePlan != null)
                        {
                            var freeSubscription = new UserSubscription
                            {
                                UserId = user.Id,
                                SubscriptionPlanId = freePlan.Id,
                                StartDate = DateTime.Now,
                                EndDate = DateTime.Now.AddYears(100), // Permanente
                                IsActive = true,
                                Status = UserSubscription.SubscriptionStatus.Active,
                                PaymentMethod = "Gratuito"
                            };
                            Crud<UserSubscription>.Create(freeSubscription);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error pero no interrumpir el registro
                        System.Diagnostics.Debug.WriteLine($"Error asignando plan gratuito: {ex.Message}");
                    }

                    try
                    {
                        await _notificationService.NotifyWelcomeAsync(user.Id, $"{user.FirstName} {user.LastName}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error enviando notificación de bienvenida: {ex.Message}");
                    }





                    // Login automático
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = GetRolesList();
            return View(model);
        }

        private List<SelectListItem> GetRolesList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "users", Text = "Usuario" },
                new SelectListItem { Value = "artists", Text = "Artista" },
                new SelectListItem { Value = "admins", Text = "Administrador" }
            };
        }

        // POST: Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData.Clear();
            return RedirectToAction("Index", "Home");
        }


        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    TempData.Clear();
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                }
            }

            return View(model);
        }

        // Redirección
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // MODIFICAR CUENTA

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                
                FirstName = user.FirstName,
                LastName = user.LastName,
                CurrentEmail = user.Email // Para mostrar email actual (no editable)
            };

            return View(model);
        }

        // POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Actualizar datos
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Perfil actualizado exitosamente";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        
        // MODIFICAR CONTRASEÑA

        // GET: Account/ChangePassword
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Contraseña cambiada exitosamente";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        // RECUPERACIÓN DE CONTRASEÑA POR EMAIL <identity>

        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            //mensaje de éxito
            ViewBag.Message = "Si el email existe, recibirás un enlace de recuperación.";

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action("ResetPassword", "Account",
                    new { token = token, email = user.Email }, Request.Scheme);

                var emailBody = $@"
                                <h2>Recuperación de Contraseña - SonikoMusic</h2>
                                <p>Hola {user.FirstName},</p>
                                <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                                <p><a href='{resetUrl}' style='background:#007bff;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Restablecer Contraseña</a></p>
                                <p>Si no solicitaste esto, ignora este email.</p>
                                <p>El enlace expira en 1 hora.</p>
                                ";

                await _emailService.SendEmailAsync(user.Email, "Recuperación de Contraseña", emailBody);
            }

            return View(model);
        }

        // GET: Account/ResetPassword
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Enlace de recuperación inválido.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword); 

            if (result.Succeeded)
            {
                TempData["Success"] = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


    }
}
