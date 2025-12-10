using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using PulperiaSystem.UI.Services;

using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace PulperiaSystem.UI
{
    // Clase auxiliar para mostrar datos en el DataGrid del Carrito
    public class ItemCarrito
    {
        public int ProductoId { get; set; }
        public required string NombreProducto { get; set; }
        public decimal Precio { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Subtotal { get { return Precio * Cantidad; } }
        
        public decimal StockMaximo { get; set; } // Referencia para validacion rapida
    }

    public partial class VentasView : UserControl
    {
        private ProductoRepository _prodRepo;
        private VentaRepository _ventaRepo;
        private List<Producto> _todosLosProductos;
        private List<ItemCarrito> _carrito;
        public Usuario UsuarioActual { get; set; } // Se asignará desde el Frame principal

        public VentasView()
        {
            InitializeComponent();
            _prodRepo = new ProductoRepository();
            _ventaRepo = new VentaRepository();
            _carrito = new List<ItemCarrito>();
            
            CargarProductos();
        }

        private void CargarProductos()
        {
            try
            {
                _todosLosProductos = _prodRepo.ObtenerTodos();
                gridProductos.ItemsSource = _todosLosProductos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}");
            }
        }

        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtBusqueda.Text.ToLower();
            if (_todosLosProductos != null)
            {
                var filtrados = _todosLosProductos.Where(p => 
                    p.Nombre.ToLower().Contains(filtro) || 
                    (p.Codigo != null && p.Codigo.ToLower().Contains(filtro))
                ).ToList();
                gridProductos.ItemsSource = filtrados;
            }
        }

        private void TxtBusqueda_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string termino = txtBusqueda.Text.Trim();
                if (string.IsNullOrEmpty(termino)) return;

                // 1. Buscar coincidencia exacta (Prioridad Código, luego Nombre)
                var producto = _todosLosProductos
                    .FirstOrDefault(p => (p.Codigo != null && p.Codigo.Equals(termino, StringComparison.OrdinalIgnoreCase)) 
                                      || p.Nombre.Equals(termino, StringComparison.OrdinalIgnoreCase));

                if (producto != null)
                {
                    // ¡Encontrado! Agregar y limpiar para el siguiente escaneo
                    AgregarAlCarrito(producto);
                    txtBusqueda.Text = "";
                    txtBusqueda.Focus();
                }
                else
                {
                    // No encontrado
                    MessageBox.Show($"Producto con código '{termino}' no encontrado.", "Scanner", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtBusqueda.SelectAll(); // Seleccionar todo para facilitar reintento
                }
            }
        }

        private void GridProductos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridProductos.SelectedItem is Producto prod)
            {
                AgregarAlCarrito(prod);
            }
        }

        private void AgregarAlCarrito(Producto prod)
        {
            if (prod.StockActual <= 0)
            {
                MessageBox.Show("Producto sin stock.", "STOCK INSUFICIENTE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Verificar si ya existe en carrito para sumar cantidad
            var existente = _carrito.FirstOrDefault(i => i.ProductoId == prod.ProductoId);
            
            // Validacion estricta de Stock
            decimal cantidadActualEnCarrito = existente?.Cantidad ?? 0;
            if (cantidadActualEnCarrito + 1 > prod.StockActual)
            {
                 MessageBox.Show($"Stock insuficiente. Solo hay {prod.StockActual} unidades.", "STOCK INSUFICIENTE", MessageBoxButton.OK, MessageBoxImage.Error);
                 return;
            }

            if (existente != null)
            {
                existente.Cantidad++;
            }
            else
            {
                _carrito.Add(new ItemCarrito
                {
                    ProductoId = prod.ProductoId,
                    NombreProducto = prod.Nombre,
                    Precio = prod.PrecioVenta,
                    Cantidad = 1,
                    StockMaximo = prod.StockActual
                });
            }

            ActualizarCarritoUI();
        }

        // HANDLERS PARA BOTONES DEL DATAGRID
        private void BtnRestarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is ItemCarrito item)
            {
                if (item.Cantidad > 1)
                {
                    item.Cantidad--;
                    ActualizarCarritoUI();
                }
                else
                {
                     // Si llega a 0, mejor preguntar si eliminar
                     if(MessageBox.Show("¿Eliminar producto del carrito?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                     {
                         _carrito.Remove(item);
                         ActualizarCarritoUI();
                     }
                }
            }
        }

        private void BtnSumarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is ItemCarrito item)
            {
                // Validar Stock
                if (item.Cantidad + 1 > item.StockMaximo)
                {
                     MessageBox.Show($"No puedes vender más de lo que hay en inventario ({item.StockMaximo}).", "STOCK INSUFICIENTE", MessageBoxButton.OK, MessageBoxImage.Error);
                     return;
                }
                item.Cantidad++;
                ActualizarCarritoUI();
            }
        }

        private void BtnEliminarItem_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is ItemCarrito item)
            {
                _carrito.Remove(item);
                ActualizarCarritoUI();
            }
        }

        private void ActualizarCarritoUI()
        {
            gridCarrito.ItemsSource = null;
            gridCarrito.ItemsSource = _carrito;

            decimal total = _carrito.Sum(i => i.Subtotal);
            txtTotal.Text = total.ToString("C2");
        }

        private void BtnCobrar_Click(object sender, RoutedEventArgs e)
        {
            if (_carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            decimal totalVenta = _carrito.Sum(i => i.Subtotal);

            // 1. ABRIR VENTANA DE COBRO
            var cobroWindow = new CobroWindow(totalVenta);
            if (cobroWindow.ShowDialog() == true && cobroWindow.CobroExitoso)
            {
                 // Si pagó correctamente, Procedemos a registrar la venta
                 ProcesarVenta(totalVenta);
            }
        }

        private void ProcesarVenta(decimal total)
        {
            // Convertir ItemCarrito a DetalleVenta
            var detalles = _carrito.Select(i => new DetalleVenta
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.Precio,
                Subtotal = i.Subtotal
            }).ToList();

            var venta = new Venta
            {
                UsuarioId = UsuarioActual?.UsuarioId ?? 1, 
                ClienteId = null, 
                Total = total,
                FormaPago = "Efectivo",
                Fecha = DateTime.Now
            };

            try
            {
                _ventaRepo.RegistrarVenta(venta, detalles); 
                
                // Asignar ID simulado si Repo no lo devuelve
                if (venta.VentaId == 0) venta.VentaId = new Random().Next(10000, 99999);

                // Preguntar Impresión
                if (MessageBox.Show("Venta registrada con éxito. ¿Desea imprimir el ticket?", "Impresión", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var ticketService = new TicketService();
                    ticketService.ImprimirTicket(venta, new List<ItemCarrito>(_carrito));
                }
            
                // Limpiar
                _carrito.Clear();
                ActualizarCarritoUI();
                CargarProductos(); 
                txtBusqueda.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la venta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
