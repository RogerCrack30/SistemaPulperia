using System;

namespace PulperiaSystem.Models
{
    public class Compra
    {
        public int CompraId { get; set; }
        public DateTime Fecha { get; set; }
        public int ProveedorId { get; set; }
        public int UsuarioId { get; set; }
        public decimal Total { get; set; }
    }
}
