using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using UserControl = System.Windows.Controls.UserControl;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class ReportesView : UserControl
    {
        private VentaRepository _ventaRepo;
        private UsuarioRepository _userRepo;

        public ReportesView()
        {
            InitializeComponent();
            _ventaRepo = new VentaRepository();
            _userRepo = new UsuarioRepository();
            
            CargarFiltros();

            dpInicio.SelectedDate = DateTime.Today;
            dpFin.SelectedDate = DateTime.Today.AddDays(1).AddSeconds(-1);
        }

        private void CargarFiltros()
        {
            try
            {
                var usuarios = _userRepo.ObtenerTodos();
                // Agregar opción "Todos"
                usuarios.Insert(0, new Usuario { UsuarioId = 0, NombreUsuario = "(Todos)" });
                
                cmbCajeros.ItemsSource = usuarios;
                cmbCajeros.SelectedIndex = 0;
            }
            catch { }
        }

        private void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            if (dpInicio.SelectedDate == null || dpFin.SelectedDate == null)
            {
                MessageBox.Show("Seleccione un rango de fechas válido.");
                return;
            }

            DateTime inicio = dpInicio.SelectedDate.Value;
            // Asegurar final del día para la fecha fin
            DateTime fin = dpFin.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);

            int? usuarioId = null;
            if (cmbCajeros.SelectedValue != null && (int)cmbCajeros.SelectedValue != 0)
            {
                usuarioId = (int)cmbCajeros.SelectedValue;
            }

            try
            {
                var reporte = _ventaRepo.ObtenerReporte(inicio, fin, usuarioId);
                gridDetalle.ItemsSource = reporte;

                // Calcular Totales en memoria (Rápido y eficiente)
                decimal totalVentas = reporte.Sum(x => x.Subtotal);
                int transacciones = reporte.Select(x => x.VentaId).Distinct().Count();
                
                // Ganancia es estimada porque el DTO no trae precio costo, por brevedad usaremos 30% estimado o 0 si no se requiere exacto.
                // Para hacerlo bien, el query debió traer PrecioCompra. 
                // Por ahora mostraremos Total Ventas que es lo que pide el usuario.
                
                txtVentasTotales.Text = totalVentas.ToString("C2");
                txtTransacciones.Text = transacciones.ToString();
                txtGanancia.Text = "---"; // Pendiente si se requiere exacto
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando reporte: " + ex.Message);
            }
        }
    }
}
