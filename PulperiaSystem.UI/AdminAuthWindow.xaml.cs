using System.Windows;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class AdminAuthWindow : Window
    {
        private UsuarioRepository _repo;
        public bool Autorizado { get; private set; } = false;

        public AdminAuthWindow()
        {
            InitializeComponent();
            _repo = new UsuarioRepository();
            txtUser.Focus();
        }

        private void txtPass_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnAutorizar_Click(sender, e);
            }
        }

        private void BtnAutorizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = _repo.Login(txtUser.Text, txtPass.Password);
                if (user != null)
                {
                    if (user.RolId == 1) // 1 = Admin
                    {
                        Autorizado = true;
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("El usuario ingresado no teine permisos de Supervisor.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Credenciales incorrectas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show("Error de conexión.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
