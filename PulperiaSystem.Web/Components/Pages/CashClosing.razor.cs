using Microsoft.AspNetCore.Components;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

namespace PulperiaSystem.Web.Components.Pages
{
    public partial class CashClosing : ComponentBase
    {
        [Inject] NavigationManager Nav { get; set; }
        [Parameter] public int UsuarioId { get; set; }

        private CajaRepository _cajaRepo = new CajaRepository();
        private CajaSesion _sesion;

        protected decimal MontoInicial { get; set; }
        protected decimal TotalVentas { get; set; }
        protected decimal TotalEsperado => MontoInicial + TotalVentas;
        
        protected decimal MontoReal { get; set; }
        protected decimal Diferencia => MontoReal - TotalEsperado;

        protected bool IsLoading { get; set; } = true;
        protected bool HasActiveSession { get; set; } = false;
        protected string ErrorMessage { get; set; }
        protected string SuccessMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadSession();
        }

        private async Task LoadSession()
        {
            IsLoading = true;
            await Task.Yield();
            try
            {
                _sesion = await Task.Run(() => _cajaRepo.VerificarCajaAbierta(UsuarioId));

                if (_sesion != null)
                {
                    HasActiveSession = true;
                    MontoInicial = _sesion.MontoInicial;
                    TotalVentas = await Task.Run(() => 
                        _cajaRepo.ObtenerTotalVentasEfectivo(UsuarioId, _sesion.FechaInicio));
                }
                else
                {
                    HasActiveSession = false;
                    ErrorMessage = "No tienes un turno abierto actualmente.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error verificando sesión: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task CerrarTurno()
        {
            if (MontoReal <= 0)
            {
                // Permitimos 0 si realmente no vendió nada, pero advertimos si es negativo? No, decimal no deja.
                // Just let it pass.
            }

            IsLoading = true;
            await Task.Yield();

            try
            {
                await Task.Run(() => 
                    _cajaRepo.CerrarCaja(_sesion.SesionId, TotalEsperado, MontoReal, Diferencia)
                );

                SuccessMessage = "Turno cerrado correctamente. Redirigiendo...";
                StateHasChanged();
                await Task.Delay(2000); // Dar tiempo a leer
                Nav.NavigateTo("/login", true);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al cerrar caja: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
