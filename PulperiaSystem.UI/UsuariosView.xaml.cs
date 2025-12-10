using System;
using System.Windows;
using System.Windows.Controls;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace PulperiaSystem.UI
{
    public partial class UsuariosView : UserControl
    {
        private UsuarioRepository _userRepo;

        public UsuariosView()
        {
            InitializeComponent();
            _userRepo = new UsuarioRepository();
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            try
            {
                gridUsuarios.ItemsSource = _userRepo.ObtenerTodos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando usuarios: " + ex.Message);
            }
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            var form = new UsuarioForm();
            if (form.ShowDialog() == true)
            {
                CargarUsuarios();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Usuario usuario)
            {
                var form = new UsuarioForm(usuario);
                if (form.ShowDialog() == true)
                {
                    CargarUsuarios();
                }
            }
        }

        private void BtnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Usuario usuario)
            {
                if(MessageBox.Show($"¿Cambiar estado (Activo/Inactivo) de {usuario.NombreUsuario}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    usuario.Estado = !usuario.Estado;
                    _userRepo.Actualizar(usuario); // Actualiza estado sin cambiar pwd
                    CargarUsuarios();
                }
            }
        }
    }
}
