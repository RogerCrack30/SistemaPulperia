using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

using UserControl = System.Windows.Controls.UserControl;
using MessageBox = System.Windows.MessageBox;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace PulperiaSystem.UI
{
    public partial class InventarioView : UserControl
    {
        private ProductoRepository _repo;

        public InventarioView()
        {
            InitializeComponent();
            _repo = new ProductoRepository();
            gridStock.LoadingRow += GridStock_LoadingRow;
            CargarStock();
        }

        private void CargarStock()
        {
            try
            {
                gridStock.ItemsSource = _repo.ObtenerTodos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar stock: " + ex.Message);
            }
        }

        private void GridStock_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var prod = e.Row.Item as Producto;
            if (prod != null && prod.StockActual <= prod.StockMinimo)
            {
                e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 200, 200)); // Rojo suave alerta
                e.Row.Foreground = Brushes.DarkRed;
            }
            else
            {
                e.Row.Background = Brushes.White;
                e.Row.Foreground = Brushes.Black;
            }
        }

        private void BtnAjuste_Click(object sender, RoutedEventArgs e)
        {
            if (gridStock.SelectedItem is Producto prod)
            {
                var window = new AjusteStockWindow(prod);
                if (window.ShowDialog() == true)
                {
                    CargarStock();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para ajustar.");
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarStock();
        }
    }
}
