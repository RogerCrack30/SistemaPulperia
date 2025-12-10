using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms; // Necesario para PrintDialog si usamos System.Drawing puro, pero en WPF mejor usar PrintDialog de WPF o envoltura.
// Sin embargo, PrintDocument es de System.Drawing.Printing.
// Para usar el dialogo nativo de impresion con PrintDocument, a veces es mas facil usar PrintDialog de WinForms o simplemente imprimir directo si se tiene nombre.
// El requerimiento dice "Abre el PrintDialog estándar de Windows".
// En WPF System.Windows.Controls.PrintDialog es para imprimir Visuals.
// Para System.Drawing.Printing.PrintDocument, se usa System.Windows.Forms.PrintDialog o se maneja manual.
// Dado que estamos en WPF, podemos usar PrintDialog de WinForms agregando referencia o usar lógica custom.
// Simplificación: Usaremos System.Windows.Forms.PrintDialog envolviendo el PrintDocument, o 
// Mejor aun: StandardPrintController para imprimir directo si se define impresora, pero el usuario quiere elegir.
// Voy a usar System.Windows.Forms para el dialogo ya que es el compañero nativo de PrintDocument.
// Nota: Necesitaremos "UseWindowsForms" en .csproj o similar si es .NET 8 WPF puro, pero System.Drawing.Common suele bastar para la logica de dibujo. 
// El dialogo UI de WinForms podría requerir configuración extra.
// Alternativa: Usar PrintDialog de WPF y adaptar, pero PrintDocument es GDI+.
// Vamos a intentar instanciar el PrintDialog de WinForms. Si falla compilacion por falta de ref, avisaré.

using PulperiaSystem.Models;

namespace PulperiaSystem.UI.Services
{
    public class TicketService
    {
        private Venta _venta;
        private List<DetalleVenta> _detalles;
        private string _nombreTienda = "PULPERIA 'EL BUEN PRECIO'";
        private string _direccion = "Barrio Central, Calle 2";

        private List<ItemCarrito> _itemsCarrito;
        
        // Sobrecarga para usar lo que ya tenemos en UI
        public void ImprimirTicket(Venta venta, List<ItemCarrito> items)
        {
            _venta = venta;
            _itemsCarrito = items;

            PrintDocument pDoc = new PrintDocument();
            pDoc.PrintPage += new PrintPageEventHandler(GenerarContenidoTicket);
            
            using (var dialog = new System.Windows.Forms.PrintDialog())
            {
                dialog.Document = pDoc;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pDoc.Print();
                }
            }
        }

        private void GenerarContenidoTicket(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Font fontRegular = new Font("Consolas", 8); // Tzequeño para 80mm
            Font fontBold = new Font("Consolas", 8, FontStyle.Bold);
            Font fontTitle = new Font("Consolas", 10, FontStyle.Bold);
            
            float y = 10;
            float offset = 0;
            float leftMargin = 0;
            float pageWidth = 280; // Aprox 80mm en puntos
            
            // 1. CABECERA
            DrawCenterText(graphics, _nombreTienda, fontTitle, pageWidth, y + offset); offset += 15;
            DrawCenterText(graphics, _direccion, fontRegular, pageWidth, y + offset); offset += 12;
            DrawCenterText(graphics, DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fontRegular, pageWidth, y + offset); offset += 12;
            DrawCenterText(graphics, $"TICKET #: {_venta.VentaId}", fontBold, pageWidth, y + offset); offset += 20;

            graphics.DrawString("------------------------------------------", fontRegular, Brushes.Black, leftMargin, y + offset); offset += 12;

            // 2. CUERPO
            foreach (var item in _itemsCarrito)
            {
                string nombre = item.NombreProducto.Length > 20 ? item.NombreProducto.Substring(0, 20) : item.NombreProducto;
                string cantidades = $"{item.Cantidad} x {item.Precio:C2}";
                string total = item.Subtotal.ToString("C2");

                graphics.DrawString(nombre, fontRegular, Brushes.Black, leftMargin, y + offset);
                // Si el nombre es largo, bajar linea, si no, a la derecha
                // Para Oxxo style simple: Nombre arriba, calculo abajo
                offset += 12;
                
                graphics.DrawString(cantidades, fontRegular, Brushes.Black, leftMargin + 10, y + offset);
                // Alineacion derecha manual para total
                float totalWidth = graphics.MeasureString(total, fontRegular).Width;
                graphics.DrawString(total, fontRegular, Brushes.Black, pageWidth - totalWidth - 10, y + offset);
                
                offset += 15;
            }

            graphics.DrawString("------------------------------------------", fontRegular, Brushes.Black, leftMargin, y + offset); offset += 12;

            // 3. PIE
            string totalStr = $"TOTAL: {_venta.Total:C2}";
            DrawCenterText(graphics, totalStr, fontTitle, pageWidth, y + offset); offset += 20;
            
            DrawCenterText(graphics, "GRACIAS POR SU COMPRA", fontBold, pageWidth, y + offset); offset += 15;
        }

        private void DrawCenterText(Graphics g, string text, Font font, float width, float y)
        {
            SizeF size = g.MeasureString(text, font);
            float x = (width - size.Width) / 2;
            g.DrawString(text, font, Brushes.Black, x, y);
        }
    }
}
