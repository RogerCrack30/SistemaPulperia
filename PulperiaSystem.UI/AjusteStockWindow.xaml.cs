using System;
using System.Windows;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class AjusteStockWindow : Window
    {
        private Producto _producto;
        private ProductoRepository _repo;

        public AjusteStockWindow(Producto producto)
        {
            InitializeComponent();
            _producto = producto;
            _repo = new ProductoRepository();

            txtProducto.Text = $"Producto: {producto.Nombre}";
            txtStockActual.Text = $"Stock Actual: {producto.StockActual} {producto.UnidadMedida}";
            txtCantidad.Focus();
        }

        private void BtnAplicar_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtCantidad.Text, out decimal cantidad))
            {
                if (cantidad == 0) return;

                try
                {
                    _repo.AjustarStock(_producto.ProductoId, cantidad);
                    MessageBox.Show("Stock ajustado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al ajustar: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Ingrese una cantidad válida.");
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
