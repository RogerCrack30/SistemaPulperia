using Microsoft.AspNetCore.Components;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

namespace PulperiaSystem.Web.Components.Pages
{
    public partial class Reports : ComponentBase
    {
        [Parameter] public int UsuarioId { get; set; }

        private VentaRepository _ventaRepo = new VentaRepository();
        
        protected DateTime FechaInicio { get; set; } = DateTime.Today;
        protected DateTime FechaFin { get; set; } = DateTime.Today;
        protected List<Venta> ReporteVentas { get; set; } = new();
        protected decimal TotalVentas { get; set; }
        protected int CantidadTransacciones { get; set; }
        protected bool IsLoading { get; set; } = false;
        protected string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GenerarReporte();
        }

        protected async Task GenerarReporte()
        {
            IsLoading = true;
            await Task.Yield();
            try
            {
                // Ajustar fin del día
                DateTime finAjustado = FechaFin.Date.AddDays(1).AddSeconds(-1);

                ReporteVentas = await Task.Run(() => 
                    _ventaRepo.ObtenerReporte(FechaInicio, finAjustado, null)
                ) ?? new List<Venta>();

                TotalVentas = ReporteVentas.Sum(v => v.Total);
                CantidadTransacciones = ReporteVentas.Count;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al generar reporte: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
