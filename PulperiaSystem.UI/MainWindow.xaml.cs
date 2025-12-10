using System.Windows;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class MainWindow : Window
    {
        private UsuarioRepository _usuarioRepo;

        public MainWindow()
        {
            InitializeComponent();
            _usuarioRepo = new UsuarioRepository();
            txtUser.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUser.Text;
            string pass = txtPass.Password;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Por favor ingrese usuario y contraseña.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var usuario = _usuarioRepo.Login(user, pass);

            if (usuario != null)
            {
                // Navegar al Menú Principal INYECTANDO el usuario real
                var menu = new MenuPrincipal(usuario);
                menu.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Credenciales incorrectas.", "Error de Login", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPass.Password = "";
                txtPass.Focus();
            }
        }

        private void txtPass_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void OnCambiarPass_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var ventana = new CambioPasswordWindow();
            ventana.ShowDialog();
        }
    }
}