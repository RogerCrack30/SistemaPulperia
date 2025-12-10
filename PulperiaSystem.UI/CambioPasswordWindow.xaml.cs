using System;
using System.Windows;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class CambioPasswordWindow : Window
    {
        private UsuarioRepository _repo;

        public CambioPasswordWindow()
        {
            InitializeComponent();
            _repo = new UsuarioRepository();
            txtUser.Focus();
        }

        private void BtnCambiar_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUser.Text;
            string oldPass = txtOldPass.Password;
            string newPass = txtNewPass.Password;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            {
                 MessageBox.Show("Todos los campos son obligatorios.");
                 return;
            }

            try
            {
                // 1. Verificar credenciales actuales
                var usuario = _repo.Login(user, oldPass);
                
                if (usuario != null)
                {
                    // 2. Verificar Rol (REGLA: SOLO ADMIN)
                    if (usuario.RolId != 1)
                    {
                        MessageBox.Show("Lo sentimos, los cajeros no tienen permiso para cambiar su contraseña desde aquí.\n\nPor favor contacte a su administrador.", "Acceso Restringido", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    // 3. Cambiar
                    // Reutilizamos el metodo Actualizar del repo, que ya maneja password si se envía
                    _repo.Actualizar(usuario, newPass);
                    
                    MessageBox.Show("Contraseña actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("El usuario o la contraseña actual son incorrectos.", "Error de Credenciales", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar contraseña: " + ex.Message);
            }
        }
    }
}
