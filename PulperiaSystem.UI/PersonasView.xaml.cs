using System;
using System.Windows;
using System.Windows.Controls;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

using UserControl = System.Windows.Controls.UserControl;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class PersonasView : UserControl
    {
        private ClienteRepository _cliRepo;
        private ProveedorRepository _provRepo;

        public PersonasView()
        {
            InitializeComponent();
            _cliRepo = new ClienteRepository();
            _provRepo = new ProveedorRepository();
            CargarClientes();
            CargarProveedores();
        }

        private void CargarClientes()
        {
            try { gridClientes.ItemsSource = _cliRepo.ObtenerTodos(); }
            catch (Exception ex) { MessageBox.Show("Error cargando clientes: " + ex.Message); }
        }

        private void CargarProveedores()
        {
            try { gridProveedores.ItemsSource = _provRepo.ObtenerTodos(); }
            catch (Exception ex) { MessageBox.Show("Error cargando proveedores: " + ex.Message); }
        }

        // CLIENTES
        private void BtnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            var form = new PersonaForm(true); // true = es cliente
            if (form.ShowDialog() == true) CargarClientes();
        }

        private void BtnEditarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (gridClientes.SelectedItem is Cliente c)
            {
                var form = new PersonaForm(c);
                if (form.ShowDialog() == true) CargarClientes();
            }
            else MessageBox.Show("Seleccione un cliente.");
        }

        private void BtnEliminarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (gridClientes.SelectedItem is Cliente c)
            {
                if (MessageBox.Show($"¿Eliminar a {c.Nombre}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _cliRepo.Eliminar(c.ClienteId);
                    CargarClientes();
                }
            }
        }

        // PROVEEDORES
        private void BtnNuevoProv_Click(object sender, RoutedEventArgs e)
        {
            var form = new PersonaForm(false); // false = es proveedor
            if (form.ShowDialog() == true) CargarProveedores();
        }

        private void BtnEditarProv_Click(object sender, RoutedEventArgs e)
        {
             if (gridProveedores.SelectedItem is Proveedor p)
            {
                var form = new PersonaForm(p);
                if (form.ShowDialog() == true) CargarProveedores();
            }
            else MessageBox.Show("Seleccione un proveedor.");
        }

        private void BtnEliminarProv_Click(object sender, RoutedEventArgs e)
        {
            if (gridProveedores.SelectedItem is Proveedor p)
            {
                if (MessageBox.Show($"¿Eliminar a {p.Nombre}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _provRepo.Eliminar(p.ProveedorId);
                    CargarProveedores();
                }
            }
        }
    }
}
