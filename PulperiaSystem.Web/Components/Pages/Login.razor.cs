using Microsoft.AspNetCore.Components;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

namespace PulperiaSystem.Web.Components.Pages
{
    public partial class Login : ComponentBase
    {
        [Inject] NavigationManager Nav { get; set; }
        
        private UsuarioRepository _repo = new UsuarioRepository();
        
        protected string Username { get; set; }
        protected string Password { get; set; }
        protected string ErrorMessage { get; set; }

        protected void HandleLogin()
        {
            try
            {
                var user = _repo.Login(Username, Password);
                if (user != null)
                {
                    // En una app real usaríamos AuthenticationStateProvider
                    // Para este MVP, pasamos el ID por URL o usamos un servicio singleton de sesion
                    Nav.NavigateTo($"/dashboard/{user.UsuarioId}");
                }
                else
                {
                    ErrorMessage = "Credenciales incorrectas.";
                }
            }
            catch
            {
                ErrorMessage = "Error de conexión con la base de datos.";
            }
        }
    }
}
