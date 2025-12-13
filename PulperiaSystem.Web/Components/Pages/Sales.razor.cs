using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

namespace PulperiaSystem.Web.Components.Pages
{
    public partial class Sales : ComponentBase
    {
        [Inject] NavigationManager Nav { get; set; } = default!;
        [Parameter] public int UsuarioId { get; set; }

        // Repositories
        private ProductoRepository _prodRepo = new ProductoRepository();
        private VentaRepository _ventaRepo = new VentaRepository();

        // State
        protected List<Producto> AllProducts { get; set; } = new();
        protected List<ItemCarritoWeb> Carrito { get; set; } = new();
        protected string SearchTerm { get; set; } = "";
        protected string ErrorMessage { get; set; } = "";
        protected string SuccessMessage { get; set; } = "";
        protected bool IsLoading { get; set; } = false;

        // Computed
        protected decimal TotalVenta => Carrito.Sum(x => x.Subtotal);

        protected override async Task OnInitializedAsync()
        {
            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            IsLoading = true;
            await Task.Yield();
            try
            {
                // Run heavy DB call in background
                AllProducts = await Task.Run(() => _prodRepo.ObtenerTodos());
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al cargar productos: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void HandleSearch(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                AgregarPorCodigo(SearchTerm);
            }
        }

        protected void BuscarYAgregar()
        {
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                AgregarPorCodigo(SearchTerm);
            }
        }

        private void AgregarPorCodigo(string termino)
        {
            termino = termino.Trim();
            var prod = AllProducts.FirstOrDefault(p => 
                (p.Codigo != null && p.Codigo.Equals(termino, StringComparison.OrdinalIgnoreCase)) ||
                p.Nombre.Equals(termino, StringComparison.OrdinalIgnoreCase));

            if (prod != null)
            {
                AgregarAlCarrito(prod);
                SearchTerm = ""; // Limpiar para siguiente escaneo
                ErrorMessage = "";
            }
            else
            {
                ErrorMessage = $"Producto '{termino}' no encontrado.";
            }
        }

        protected void AgregarAlCarrito(Producto prod)
        {
            if (prod.StockActual <= 0)
            {
                ErrorMessage = $"El producto {prod.Nombre} NO tiene stock.";
                return;
            }

            var item = Carrito.FirstOrDefault(i => i.ProductoId == prod.ProductoId);
            if (item != null)
            {
                if (item.Cantidad + 1 > prod.StockActual)
                {
                    ErrorMessage = $"Stock insuficiente de {prod.Nombre}.";
                    return;
                }
                item.Cantidad++;
            }
            else
            {
                Carrito.Add(new ItemCarritoWeb
                {
                    ProductoId = prod.ProductoId,
                    Nombre = prod.Nombre,
                    Precio = prod.PrecioVenta,
                    Cantidad = 1,
                    StockMaximo = prod.StockActual
                });
            }
        }

        protected void AumentarCantidad(ItemCarritoWeb item)
        {
            if (item.Cantidad + 1 <= item.StockMaximo)
            {
                item.Cantidad++;
            }
            else
            {
                ErrorMessage = $"Máximo stock alcanzado para {item.Nombre}.";
            }
        }

        protected void DisminuirCantidad(ItemCarritoWeb item)
        {
            if (item.Cantidad > 1)
            {
                item.Cantidad--;
            }
            else
            {
                Carrito.Remove(item);
            }
        }

        protected void EliminarItem(ItemCarritoWeb item)
        {
            Carrito.Remove(item);
        }

        protected async Task ProcesarVenta()
        {
            if (!Carrito.Any()) return;

            IsLoading = true;
            await Task.Yield();

            try
            {
                var venta = new Venta
                {
                    UsuarioId = UsuarioId > 0 ? UsuarioId : 1, // Fallback a admin si no hay user
                    Total = TotalVenta,
                    FormaPago = "Efectivo",
                    Fecha = DateTime.Now
                };

                var detalles = Carrito.Select(i => new DetalleVenta
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.Precio,
                    Subtotal = i.Subtotal
                }).ToList();

                await Task.Run(() => _ventaRepo.RegistrarVenta(venta, detalles));

                SuccessMessage = $"¡Venta registrada! Total: {venta.Total:C2}";
                Carrito.Clear();
                await LoadProducts(); // Recargar inventario
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al procesar venta: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class ItemCarritoWeb
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Subtotal => Precio * Cantidad;
        public decimal StockMaximo { get; set; }
    }
}
