using Microsoft.AspNetCore.Components;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

namespace PulperiaSystem.Web.Components.Pages
{
    public partial class Inventory : ComponentBase
    {
        [Inject] NavigationManager Nav { get; set; }
        [Parameter] public int UsuarioId { get; set; }

        private ProductoRepository _prodRepo = new ProductoRepository();
        
        protected List<Producto> AllProducts { get; set; } = new();
        protected List<Producto> FilteredProducts { get; set; } = new();
        protected string SearchTerm { get; set; } = "";
        protected bool IsLoading { get; set; } = true;
        protected string ErrorMessage { get; set; }

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
                AllProducts = await Task.Run(() => _prodRepo.ObtenerTodos());
                FilterProducts();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al cargar inventario: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void HandleSearch(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? "";
            FilterProducts();
        }

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                FilteredProducts = new List<Producto>(AllProducts);
            }
            else
            {
                FilteredProducts = AllProducts
                    .Where(p => p.Nombre.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                                (p.Codigo != null && p.Codigo.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
        }

        protected void GoToCreate()
        {
            Nav.NavigateTo($"/product/new/{UsuarioId}");
        }

        protected void GoToEdit(int id)
        {
            Nav.NavigateTo($"/product/edit/{id}/{UsuarioId}");
        }
    }
}
