using System;
using System.Windows;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PulperiaSystem.UI
{
    public partial class ProductoForm : Window
    {
        private ProductoRepository _prodRepo;
        private CategoriaRepository _catRepo;
        private Producto _productoActual;

        public ProductoForm(Producto producto = null)
        {
            InitializeComponent();
            _prodRepo = new ProductoRepository();
            _catRepo = new CategoriaRepository();
            _productoActual = producto;

            CargarCategorias();
            if (_productoActual != null)
            {
                CargarDatos();
            }
        }

        private void CargarCategorias()
        {
            try
            {
                cmbCategoria.ItemsSource = _catRepo.ObtenerTodas();
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Error cargando categorías: " + ex.Message);
            }
        }

        private void CargarDatos()
        {
            txtCodigo.Text = _productoActual.Codigo;
            txtNombre.Text = _productoActual.Nombre;
            cmbCategoria.SelectedValue = _productoActual.CategoriaId;
            txtCompra.Text = _productoActual.PrecioCompra.ToString();
            txtVenta.Text = _productoActual.PrecioVenta.ToString();
            txtStock.Text = _productoActual.StockActual.ToString();
            txtMinStock.Text = _productoActual.StockMinimo.ToString();
            txtUnidad.Text = _productoActual.UnidadMedida;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Nombre y Categoría son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validaciones
            if (!decimal.TryParse(txtVenta.Text, out decimal pV) || pV <= 0) { MessageBox.Show("Precio inválido"); return; }
            if (!decimal.TryParse(txtStock.Text, out decimal st) || st < 0) { MessageBox.Show("Stock inválido"); return; }

            if (_productoActual == null) _productoActual = new Producto();

            _productoActual.Codigo = txtCodigo.Text;
            _productoActual.Nombre = txtNombre.Text;
            _productoActual.CategoriaId = (int)cmbCategoria.SelectedValue;
            
            decimal.TryParse(txtCompra.Text, out decimal pCompra);
            _productoActual.PrecioCompra = pCompra;

            decimal.TryParse(txtVenta.Text, out decimal pVenta);
            _productoActual.PrecioVenta = pVenta;

            decimal.TryParse(txtStock.Text, out decimal stock);
            _productoActual.StockActual = stock;

            decimal.TryParse(txtMinStock.Text, out decimal minStock);
            _productoActual.StockMinimo = minStock;

            _productoActual.UnidadMedida = txtUnidad.Text;

            try
            {
                if (_productoActual.ProductoId == 0)
                {
                    _prodRepo.Agregar(_productoActual);
                }
                else
                {
                    _prodRepo.Actualizar(_productoActual);
                }
                
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}");
            }
        }

        private void TxtCodigo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string codigo = txtCodigo.Text.Trim();
                if (string.IsNullOrEmpty(codigo)) return;

                // Verificar si ya existe para cargar datos
                try 
                {
                    var productos = _prodRepo.ObtenerTodos(); // Ineficiente pero functional por ahora
                    var existente = productos.Find(p => p.Codigo == codigo);
                    
                    if (existente != null)
                    {
                        if(MessageBox.Show("Este código ya existe. ¿Desea editar el producto?", "Producto Existente", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            _productoActual = existente;
                            CargarDatos();
                        }
                    }
                    else
                    {
                        // Nuevo producto, saltar al nombre
                        txtNombre.Focus();
                    }
                }
                catch { }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
