using System;
using System.Windows;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace PulperiaSystem.UI
{
    public partial class CobroWindow : Window
    {
        public decimal TotalPagar { get; private set; }
        public decimal MontoRecibido { get; private set; }
        public decimal Cambio { get; private set; }

        public bool CobroExitoso { get; private set; } = false;

        public CobroWindow(decimal total)
        {
            InitializeComponent();
            TotalPagar = total;
            lblTotal.Text = TotalPagar.ToString("C2");
            txtRecibido.Focus();
        }

        private void TxtRecibido_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmarCobro();
            }
        }

        private void BtnCobrar_Click(object sender, RoutedEventArgs e)
        {
            ConfirmarCobro();
        }

        private void ConfirmarCobro()
        {
            if (decimal.TryParse(txtRecibido.Text, out decimal recibido))
            {
                if (recibido < TotalPagar)
                {
                    decimal falta = TotalPagar - recibido;
                    MessageBox.Show($"Monto insuficiente. Faltan: {falta:C2}", "Error de Pago", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MontoRecibido = recibido;
                Cambio = MontoRecibido - TotalPagar;
                
                lblCambio.Text = Cambio.ToString("C2");
                lblCambio.Visibility = Visibility.Visible;
                lblTituloCambio.Visibility = Visibility.Visible;

                MessageBox.Show($"¡Cobro Exitoso!\n\nEntregar Cambio: {Cambio:C2}", "Pago Procesado", MessageBoxButton.OK, MessageBoxImage.Information);

                CobroExitoso = true;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ingrese un monto válido.");
            }
        }

        private void TxtRecibido_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtRecibido.Text, out decimal val))
            {
                if (val >= TotalPagar)
                {
                    lblCambioPrevia.Text = (val - TotalPagar).ToString("C2");
                }
                else
                {
                    lblCambioPrevia.Text = "---";
                }
            }
        }
    }
}
