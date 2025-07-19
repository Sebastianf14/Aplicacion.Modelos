using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos.Suscription
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } // "Gratuito", "Personal", "Familiar", "Empresarial"
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMonths { get; set; } // 1, 6, 12
        public bool AllowDownloads { get; set; }
        public bool AllowOfflineMode { get; set; }
        public int MaxPlaylists { get; set; } // -1 = ilimitado
        public bool HasAds { get; set; }
        public bool IsActive { get; set; } = true;

        // Navegación
        public List<UserSubscription>? UserSubscriptions { get; set; }
    }
}
