using System;

namespace PulperiaSystem.Models
{
    public class MovimientoCaja
    {
        public int MovimientoId { get; set; }
        public int SesionId { get; set; }
        public string Tipo { get; set; } // 'INGRESO' o 'RETIRO'
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
