using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.Modelos
{
    public class Suscripcion
    {
        [Key] public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public double Monto { get; set; }
        public Metodo MetodoPago { get; set; }
        public Estado EstadoPago { get; set; }
        public bool EsActiva { get; set; }

        //
        public string UsuarioId { get; set; }
        public int PlanId { get; set; }
        // 
        public User Usuario { get; set; }
        public Plan Plan { get; set; }
    }
    public enum Metodo
    {
        TarjetaCredito = 1,
        TarjetaDebito = 2,
        PayPal = 3,
        Transferencia = 4
    }

    public enum Estado
    {
        Pendiente = 1,
        Completado = 2,
        Fallido = 3,
        Cancelado = 4,
        Reembolsado = 5
    }
}
