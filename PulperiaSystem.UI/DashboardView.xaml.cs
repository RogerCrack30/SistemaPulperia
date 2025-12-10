using System;
using System.Windows;
using System.Windows.Controls;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace PulperiaSystem.UI
{
    public partial class DashboardView : UserControl
    {
        private DashboardRepository _dashboardRepo;

        public DashboardView()
        {
            InitializeComponent();
            _dashboardRepo = new DashboardRepository();
            CargarDatos();
        }

        public void CargarDatos()
        {
            try
            {
                // 1. KPIs
                decimal totalVentas = _dashboardRepo.ObtenerTotalVentasHoy();
                int cantVentas = _dashboardRepo.ObtenerCantidadVentasHoy();
                
                lblVentasHoy.Text = totalVentas.ToString("C2");
                lblTransacciones.Text = cantVentas.ToString();

                // 2. Alertas de Stock
                var bajoStock = _dashboardRepo.ObtenerProductosBajoStock();
                gridBajoStock.ItemsSource = bajoStock;
                
                lblCountBajoStock.Text = bajoStock.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar Dashboard: " + ex.Message);
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }
    }
}
