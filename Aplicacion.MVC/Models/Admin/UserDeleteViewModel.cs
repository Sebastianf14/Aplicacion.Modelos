namespace Aplicacion.MVC.Models.Admin
{
    public class UserDeleteViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }

        // Contenido a eliminar
        public int MusicCount { get; set; }
        public int AlbumCount { get; set; }
        public int PlaylistCount { get; set; }
        public int DownloadCount { get; set; }
    }
}
