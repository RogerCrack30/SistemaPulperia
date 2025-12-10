using System;
using System.Windows;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class AperturaCajaWindow : Window
    {
        private CajaRepository _cajaRepo;
        public Usuario UsuarioActual { get; set; }

        public AperturaCajaWindow(Usuario usuario)
        {
            InitializeComponent();
            _cajaRepo = new CajaRepository();
            UsuarioActual = usuario;
            txtMonto.Focus();
        }

        private void BtnAbrir_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMonto.Text, out decimal monto))
            {
                if (monto < 0)
                {
                    MessageBox.Show("El monto no puede ser negativo.");
                    return;
                }

                try
                {
                    _cajaRepo.AbrirCaja(UsuarioActual.UsuarioId, monto);
                    MessageBox.Show("¡Turno abierto correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al abrir caja: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Ingrese un monto válido.");
            }
        }
    }
}
