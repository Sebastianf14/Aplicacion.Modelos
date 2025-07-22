namespace Aplicacion.MVC.Models.Admin
{
    public class UserListViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Role { get; set; }

        // Estadísticas específicas por rol
        public int MusicCount { get; set; } // Para artistas
        public int AlbumCount { get; set; } // Para artistas
        public int PlaylistCount { get; set; } // Para usuarios
        public string SubscriptionPlan { get; set; } // Para usuarios
        public int DownloadCount { get; set; } // Para usuarios
        public int TotalPlays { get; set; } // Para métricas futuras
    }
}
