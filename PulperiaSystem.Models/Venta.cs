using System;

namespace PulperiaSystem.Models
{
    public class Venta
    {
        public int VentaId { get; set; }
        public DateTime Fecha { get; set; }
        public int UsuarioId { get; set; }
        public int? ClienteId { get; set; }
        public decimal Total { get; set; }
        public string FormaPago { get; set; }
        public string Estado { get; set; }
    }
}
