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


        protected bool IsLoading { get; set; } = false;

        protected async Task HandleLogin()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = "";
            StateHasChanged();

            // Give UI a moment to update
            await Task.Yield();

            try
            {
                // Run sync repo in background thread to avoid blocking UI
                var user = await Task.Run(() => _repo.Login(Username, Password));

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
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex}");
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
