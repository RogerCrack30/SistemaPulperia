using System;

namespace PulperiaSystem.Models
{
    public class CajaSesion
    {
        public int SesionId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal? MontoFinalEsperado { get; set; }
        public decimal? MontoFinalReal { get; set; }
        public decimal? Diferencia { get; set; }
        public bool Estado { get; set; } // true (1) = Abierta, false (0) = Cerrada
    }
}
