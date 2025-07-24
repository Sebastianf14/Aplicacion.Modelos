using Aplicacion.API.Consumer;
using Aplicacion.Modelos.Suscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aplicacion.MVC.Controllers
{
    [Authorize(Roles = "admins")]
    public class SubscriptionPlansController : Controller
    {
        // GET: SubscriptionPlansController
        public ActionResult Index()
        {
            
            var data = Crud<SubscriptionPlan>.GetAll();
            return View(data);
        }

        // GET: SubscriptionPlansController/Details/5
        public ActionResult Details(int id)
        {
            var data = Crud<SubscriptionPlan>.GetById(id);
            return View(data);
        }

        // GET: SubscriptionPlansController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SubscriptionPlansController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SubscriptionPlan data)
        {
            try
            {
                Crud<SubscriptionPlan>.Create(data);
                TempData["Success"] = "Plan de suscripción creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: SubscriptionPlansController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<SubscriptionPlan>.GetById(id);
            return View(data);
        }

        // POST: SubscriptionPlansController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, SubscriptionPlan data)
        {
            try
            {
                Crud<SubscriptionPlan>.Update(id, data);
                TempData["Success"] = "Plan actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: SubscriptionPlansController/Delete/5
        public ActionResult Delete(int id)
        {
            var data = Crud<SubscriptionPlan>.GetById(id);
            return View(data);
        }

        // POST: SubscriptionPlansController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, SubscriptionPlan data)
        {
            try
            {
                Crud<SubscriptionPlan>.Delete(id);
                TempData["Success"] = "Plan eliminado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: SubscriptionPlans/Toggle/5 - Activar/Desactivar plan
        public ActionResult Toggle(int id)
        {
            try
            {
                var plan = Crud<SubscriptionPlan>.GetById(id);
                if (plan != null)
                {
                    plan.IsActive = !plan.IsActive;
                    Crud<SubscriptionPlan>.Update(id, plan);
                    TempData["Success"] = $"Plan {(plan.IsActive ? "activado" : "desactivado")} exitosamente";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }








    }
}
