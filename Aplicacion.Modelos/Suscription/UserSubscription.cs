using Aplicacion.Modelos.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos.Suscription
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public string? PaymentMethod { get; set; } // "Credit Card", "PayPal", etc.

        // Navegación
        public User? User { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }

        public enum SubscriptionStatus
        {
            Active = 1,
            Expired = 2,
            Cancelled = 3,
            Pending = 4
        }
    }
}
