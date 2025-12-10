using System;
using System.Windows;
using System.Windows.Controls;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class UsuarioForm : Window
    {
        private UsuarioRepository _repo;
        private Usuario _usuarioActual;

        public UsuarioForm(Usuario usuario = null)
        {
            InitializeComponent();
            _repo = new UsuarioRepository();
            _usuarioActual = usuario;

            if (_usuarioActual != null)
            {
                CargarDatos();
            }
        }

        private void CargarDatos()
        {
            txtUsuario.Text = _usuarioActual.NombreUsuario;
            
            // Seleccionar Rol
            foreach(ComboBoxItem item in cmbRol.Items)
            {
                if (item.Tag.ToString() == _usuarioActual.RolId.ToString())
                {
                    cmbRol.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MessageBox.Show("El usuario es obligatorio.");
                return;
            }

            if (_usuarioActual == null)
            {
                // NUEVO
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("La contraseña es obligatoria para nuevos usuarios.");
                    return;
                }

                var nuevo = new Usuario
                {
                    NombreUsuario = txtUsuario.Text,
                    RolId = int.Parse(((ComboBoxItem)cmbRol.SelectedItem).Tag.ToString())
                };
                
                try
                {
                    _repo.Agregar(nuevo, txtPassword.Password);
                    DialogResult = true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message);
                }
            }
            else
            {
                // EDITAR
                _usuarioActual.NombreUsuario = txtUsuario.Text;
                _usuarioActual.RolId = int.Parse(((ComboBoxItem)cmbRol.SelectedItem).Tag.ToString());

                try
                {
                    string pass = string.IsNullOrWhiteSpace(txtPassword.Password) ? null : txtPassword.Password;
                    _repo.Actualizar(_usuarioActual, pass);
                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al actualizar: " + ex.Message);
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
