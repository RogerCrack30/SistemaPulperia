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
    public partial class ProductosView : UserControl
    {
        private ProductoRepository _repo;

        public ProductosView()
        {
            InitializeComponent();
            _repo = new ProductoRepository();
            gridInventario.LoadingRow += GridInventario_LoadingRow;
            CargarProductos();
        }

        private void CargarProductos()
        {
            try
            {
                var lista = _repo.ObtenerTodos();
                gridInventario.ItemsSource = lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando inventario: " + ex.Message);
            }
        }

        private void GridInventario_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var prod = e.Row.Item as Producto;
            if (prod != null)
            {
                if (prod.StockActual <= prod.StockMinimo)
                {
                    e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230)); // Rojo claro
                    e.Row.Foreground = Brushes.DarkRed;
                }
                else
                {
                     e.Row.Background = Brushes.White;
                     e.Row.Foreground = Brushes.Black;
                }
            }
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            var form = new ProductoForm(null);
            if (form.ShowDialog() == true)
            {
                CargarProductos();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (gridInventario.SelectedItem is Producto prod)
            {
                var form = new ProductoForm(prod);
                if (form.ShowDialog() == true)
                {
                    CargarProductos();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para editar.");
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (gridInventario.SelectedItem is Producto prod)
            {
                if (MessageBox.Show($"¿Eliminar {prod.Nombre}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repo.Eliminar(prod.ProductoId);
                        CargarProductos();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error al eliminar: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para eliminar.");
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarProductos();
        }
    }
}
