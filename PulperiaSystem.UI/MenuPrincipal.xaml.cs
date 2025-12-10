using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class MenuPrincipal : Window
    {
        private Usuario _usuarioActual; // Necesitamos simular o traer el usuario
        private CajaRepository _cajaRepo;

        public MenuPrincipal(Usuario usuario) // Inyectar usuario real
        {
            InitializeComponent();
            _cajaRepo = new CajaRepository();
            _usuarioActual = usuario;

            CargarConfiguracion();
            AplicarPermisos();
            
            // Cargar Dashboard por defecto
            MainContent.Content = new DashboardView();
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            txtTituloVista.Text = "Panel de Control";
            MainContent.Content = new DashboardView();
        }

        // Constructor sin parametros para compatibilidad XAML (si se usa) o pruebas, pero ideal usar el otro
        public MenuPrincipal() : this(new Usuario { UsuarioId = 1, NombreUsuario = "AdminDefault", RolId = 1 }) { }

        private void CargarConfiguracion()
        {
            try
            {
                var configRepo = new ConfiguracionRepository();
                string nombreTienda = configRepo.ObtenerValor("NombreTienda");
                if (!string.IsNullOrEmpty(nombreTienda))
                {
                    this.Title = "Sistema - " + nombreTienda;
                    txtTituloApp.Text = nombreTienda.ToUpper(); // Asumiendo que hay un TextBlock para el titulo en XAML
                }
            }
            catch { /* Fallback silencioso */ }
        }

        private void AplicarPermisos()
        {
            lblUsuario.Text = $"Usuario: {_usuarioActual.NombreUsuario}";
            
            // Lógica Centralizada de Roles
            bool esAdmin = _usuarioActual.RolId == 1;
            bool esCajero = _usuarioActual.RolId == 2;

            if (esCajero)
            {
                // Cajero solo ve Ventas y Dashboard (Inicio)
                BtnProductos.Visibility = Visibility.Collapsed;
                BtnInventario.Visibility = Visibility.Collapsed;
                BtnPersonas.Visibility = Visibility.Collapsed; 
                BtnReportes.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed; 
            }
            
            if (esAdmin)
            {
                // Admin ve todo, asegurar que Gestión Usuarios sea visible
                BtnUsuarios.Visibility = Visibility.Visible;
            }
        }

        private void BtnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            // CANDADO DE SEGURIDAD: Doble verificación
            if (_usuarioActual.RolId != 1) 
            {
                MessageBox.Show("Acceso Restringido. Solo Administradores.", "Seguridad", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            txtTituloVista.Text = "Gestión de Usuarios";
            MainContent.Content = new UsuariosView();
        }

        private void BtnVentas_Click(object sender, RoutedEventArgs e)
        {
            // VALIDACION STRICTA: ¿Tiene turno abierto?
            var sesion = _cajaRepo.VerificarCajaAbierta(_usuarioActual.UsuarioId);
            
            if (sesion == null)
            {
                // NO TIENE CAJA ABIERTA -> OBLIGAR APERTURA
                var apertura = new AperturaCajaWindow(_usuarioActual);
                if (apertura.ShowDialog() == true)
                {
                    // Si abrió correctamente, permitir entrar.
                    txtTituloVista.Text = "Punto de Venta";
                    var vistaVentas = new VentasView();
                    vistaVentas.UsuarioActual = _usuarioActual; // Pasar usuario
                    MainContent.Content = vistaVentas;
                }
                else
                {
                    // Cancelo apertura -> No entrar
                    MessageBox.Show("Se requiere una caja abierta para vender.");
                }
            }
            else
            {
                // YA TIENE CAJA ABIERTA -> PASAR
                txtTituloVista.Text = "Punto de Venta";
                var vistaVentas = new VentasView();
                vistaVentas.UsuarioActual = _usuarioActual;
                MainContent.Content = vistaVentas;
            }
        }

        private void BtnProductos_Click(object sender, RoutedEventArgs e)
        {
            txtTituloVista.Text = "Gestión de Productos";
            MainContent.Content = new ProductosView();
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            txtTituloVista.Text = "Control de Inventario";
             MainContent.Content = new InventarioView();
        }
        
        private void BtnPersonas_Click(object sender, RoutedEventArgs e)
        {
            txtTituloVista.Text = "Clientes y Proveedores";
            MainContent.Content = new PersonasView();
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            txtTituloVista.Text = "Reportes y Estadísticas";
            MainContent.Content = new ReportesView();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            var sesion = _cajaRepo.VerificarCajaAbierta(_usuarioActual.UsuarioId);
            if (sesion != null)
            {
                // OFRECER CERRA TURNO
                if (MessageBox.Show("Tiene un turno abierto. ¿Desea cerrar caja antes de salir?", "Turno Activo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    txtTituloVista.Text = "Cierre de Caja";
                    var cierreView = new CierreCajaView();
                    cierreView.Inicializar(_usuarioActual);
                    MainContent.Content = cierreView;
                    return; // No salir todavía, mostrar la vista
                }
            }

            var login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}
