using System;
using System.Windows;
using System.Windows.Controls;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using UserControl = System.Windows.Controls.UserControl;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class CierreCajaView : UserControl
    {
        private CajaRepository _cajaRepo;
        private CajaSesion _sesionActual;
        private Usuario _usuario;

        private decimal _calculado = 0;

        public CierreCajaView()
        {
            InitializeComponent();
            _cajaRepo = new CajaRepository();
        }

        public void Inicializar(Usuario usuario)
        {
            _usuario = usuario;
            CargarDatosSession();
        }

        private void CargarDatosSession()
        {
            try
            {
                _sesionActual = _cajaRepo.VerificarCajaAbierta(_usuario.UsuarioId);
                if (_sesionActual == null)
                {
                    MessageBox.Show("No hay sesión activa para cerrar.");
                    return;
                }

                // Calcular
                decimal ventas = _cajaRepo.ObtenerTotalVentasEfectivo(_usuario.UsuarioId, _sesionActual.FechaInicio);
                decimal saldoInicial = _sesionActual.MontoInicial;
                decimal retiros = 0; 

                _calculado = saldoInicial + ventas - retiros;

                lblFechaInicio.Text = _sesionActual.FechaInicio.ToString("dd/MM/yyyy HH:mm");
                lblMontoInicial.Text = saldoInicial.ToString("C2");
                lblVentas.Text = ventas.ToString("C2");
                lblTotalEsperado.Text = _calculado.ToString("C2");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos de sesión: " + ex.Message);
            }
        }

        private void BtnCerrarTurno_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMontoReal.Text, out decimal real))
            {
                decimal diferencia = real - _calculado;

                string msg = $"Resumen del Cierre:\n\n" +
                             $"Esperado: {_calculado:C2}\n" +
                             $"Real: {real:C2}\n\n" +
                             $"Diferencia: {diferencia:C2}\n\n" +
                             $"¿Está seguro de cerrar el turno?";

                var result = MessageBox.Show(msg, "Confirmar Cierre Z", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    // VERIFICACION: Si es CAJERO, pedir Supervisor
                    if (_usuario.RolId == 2)
                    {
                        var auth = new AdminAuthWindow();
                        if (auth.ShowDialog() == true && auth.Autorizado)
                        {
                             // Autorizado, continuar
                        }
                        else
                        {
                            MessageBox.Show("Cierre cancelado. Se requiere autorización de Supervisor.", "Autorización Fallida", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                    }

                    try
                    {
                        _cajaRepo.CerrarCaja(_sesionActual.SesionId, _calculado, real, diferencia);
                        MessageBox.Show("Turno cerrado correctamente. Sesión finalizada.");
                        
                        var login = new MainWindow();
                        login.Show();
                        Window.GetWindow(this).Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al cerrar caja: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Ingrese el monto contado en caja (Corte Ciego).");
            }
        }
    }
}
