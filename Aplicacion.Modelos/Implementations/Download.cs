using Aplicacion.Modelos.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos.Implementations
{
    public class Download
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MusicId { get; set; }
        public DateTime DownloadDate { get; set; } = DateTime.Now;
        public string? LocalFilePath { get; set; } // Ruta donde se guardó en el dispositivo del usuario
        public DownloadStatus Status { get; set; } = DownloadStatus.Completed;

        // Navegación
        public User? User { get; set; }
        public Musica? Music { get; set; }

        public enum DownloadStatus
        {
            Pending = 1,
            InProgress = 2,
            Completed = 3,
            Failed = 4,
            Deleted = 5
        }
    }
}
