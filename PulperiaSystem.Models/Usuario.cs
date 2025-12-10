namespace PulperiaSystem.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string ContrasenaHash { get; set; }
        public int RolId { get; set; }
        public bool Estado { get; set; }
    }
}
