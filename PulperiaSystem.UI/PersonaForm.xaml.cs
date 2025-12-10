using System;
using System.Windows;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class PersonaForm : Window
    {
        private bool _esCliente;
        private object _entidad;
        private ClienteRepository _clienteRepo;
        private ProveedorRepository _provRepo;

        // Constructor para Nuevo
        public PersonaForm(bool esCliente)
        {
            InitializeComponent();
            _esCliente = esCliente;
            InicializarRepos();
            txtTitulo.Text = esCliente ? "Nuevo Cliente" : "Nuevo Proveedor";
        }

        // Constructor para Editar
        public PersonaForm(object entidad)
        {
            InitializeComponent();
            _entidad = entidad;
            InicializarRepos();
            
            if (entidad is Cliente c)
            {
                _esCliente = true;
                txtTitulo.Text = "Editar Cliente";
                txtNombre.Text = c.Nombre;
                txtTelefono.Text = c.Telefono;
                txtDireccion.Text = c.Direccion;
            }
            else if (entidad is Proveedor p)
            {
                _esCliente = false;
                txtTitulo.Text = "Editar Proveedor";
                txtNombre.Text = p.Nombre;
                txtTelefono.Text = p.Telefono;
                txtDireccion.Text = p.Direccion;
            }
        }

        private void InicializarRepos()
        {
            _clienteRepo = new ClienteRepository();
            _provRepo = new ProveedorRepository();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.");
                return;
            }

            try
            {
                if (_esCliente)
                {
                    var c = (_entidad as Cliente) ?? new Cliente();
                    c.Nombre = txtNombre.Text;
                    c.Telefono = txtTelefono.Text;
                    c.Direccion = txtDireccion.Text;

                    if (c.ClienteId == 0) _clienteRepo.Agregar(c);
                    else _clienteRepo.Actualizar(c);
                }
                else
                {
                    var p = (_entidad as Proveedor) ?? new Proveedor();
                    p.Nombre = txtNombre.Text;
                    p.Telefono = txtTelefono.Text;
                    p.Direccion = txtDireccion.Text;

                    if (p.ProveedorId == 0) _provRepo.Agregar(p);
                    else _provRepo.Actualizar(p);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
