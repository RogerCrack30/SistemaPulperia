using Microsoft.AspNetCore.Components;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

namespace PulperiaSystem.Web.Components.Pages
{
    public partial class ProductForm : ComponentBase
    {
        [Inject] NavigationManager Nav { get; set; } = default!;
        
        [Parameter] public int UsuarioId { get; set; }
        [Parameter] public int? Id { get; set; } // Null for New, Int for Edit

        private ProductoRepository _prodRepo = new ProductoRepository();
        private CategoriaRepository _catRepo = new CategoriaRepository();

        protected Producto Model { get; set; } = new Producto();
        protected List<Categoria> Categorias { get; set; } = new();
        protected string ErrorMessage { get; set; } = "";
        protected bool IsLoading { get; set; } = false;

        protected string Title => Id.HasValue ? "Editar Producto" : "Nuevo Producto";

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            IsLoading = true;
            await Task.Yield();
            try
            {
                Categorias = await Task.Run(() => _catRepo.ObtenerTodas());

                if (Id.HasValue && Id.Value > 0)
                {
                    // Edit Mode: Load existing product
                     var all = await Task.Run(() => _prodRepo.ObtenerTodos());
                     var existing = all.FirstOrDefault(p => p.ProductoId == Id.Value);
                     if (existing != null)
                     {
                         Model = existing;
                     }
                     else
                     {
                         ErrorMessage = "Producto no encontrado.";
                     }
                }
                else
                {
                    // Create Mode: Init defaults
                    Model = new Producto 
                    { 
                        StockMinimo = 5, 
                        UnidadMedida = "Unidad",
                        CategoriaId = Categorias.FirstOrDefault()?.CategoriaId ?? 0 
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error cargando datos: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task HandleSave()
        {
            if (string.IsNullOrWhiteSpace(Model.Nombre))
            {
                ErrorMessage = "El nombre es obligatorio.";
                return;
            }

            if (Model.CategoriaId <= 0)
            {
                 ErrorMessage = "Seleccione una categoría.";
                 return;
            }

            IsLoading = true;
            await Task.Yield();

            try
            {
                await Task.Run(() => 
                {
                    if (Id.HasValue && Id.Value > 0)
                    {
                        _prodRepo.Actualizar(Model);
                    }
                    else
                    {
                        _prodRepo.Agregar(Model);
                    }
                });

                Nav.NavigateTo($"/inventory/{UsuarioId}");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al guardar: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void Cancel()
        {
            Nav.NavigateTo($"/inventory/{UsuarioId}");
        }
    }
}
