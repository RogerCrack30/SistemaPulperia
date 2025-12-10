using System;

namespace PulperiaSystem.Models
{
    public class ReporteVentaItem
    {
        public int VentaId { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Producto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal { get; set; }
    }
}
