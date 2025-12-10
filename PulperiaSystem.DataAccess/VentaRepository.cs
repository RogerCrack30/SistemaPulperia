using System;
using System.Collections.Generic;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class VentaRepository
    {
        public bool RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar Encabezado de Venta
                        // POSTGRES: OUTPUT INSERTED.VentaId => RETURNING VentaId
                        string queryVenta = @"
                            INSERT INTO Ventas (UsuarioId, ClienteId, Total, FormaPago) 
                            VALUES (@UserId, @CliId, @Total, @FormaPago)
                            RETURNING VentaId";

                        int ventaId;
                        using (var cmdVenta = new NpgsqlCommand(queryVenta, connection, transaction))
                        {
                            cmdVenta.Parameters.AddWithValue("UserId", venta.UsuarioId);
                            cmdVenta.Parameters.AddWithValue("CliId", (object)venta.ClienteId ?? DBNull.Value);
                            cmdVenta.Parameters.AddWithValue("Total", venta.Total);
                            cmdVenta.Parameters.AddWithValue("FormaPago", venta.FormaPago);
                            
                            ventaId = (int)cmdVenta.ExecuteScalar();
                        }

                        // 2. Insertar Detalles y Actualizar Stock
                        foreach (var det in detalles)
                        {
                            // A. Insertar Detalle
                            string queryDetalle = @"
                                INSERT INTO DetalleVentas (VentaId, ProductoId, Cantidad, PrecioUnitario, Subtotal)
                                VALUES (@VentaId, @ProdId, @Cant, @Prec, @Sub)";
                            
                            using (var cmdDetalle = new NpgsqlCommand(queryDetalle, connection, transaction))
                            {
                                cmdDetalle.Parameters.AddWithValue("VentaId", ventaId);
                                cmdDetalle.Parameters.AddWithValue("ProdId", det.ProductoId);
                                cmdDetalle.Parameters.AddWithValue("Cant", det.Cantidad);
                                cmdDetalle.Parameters.AddWithValue("Prec", det.PrecioUnitario);
                                cmdDetalle.Parameters.AddWithValue("Sub", det.Subtotal);
                                cmdDetalle.ExecuteNonQuery();
                            }

                            // B. Actualizar Stock
                            string queryStock = "UPDATE Productos SET StockActual = StockActual - @Cant WHERE ProductoId = @ProdId";
                            using (var cmdStock = new NpgsqlCommand(queryStock, connection, transaction))
                            {
                                cmdStock.Parameters.AddWithValue("Cant", det.Cantidad);
                                cmdStock.Parameters.AddWithValue("ProdId", det.ProductoId);
                                cmdStock.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw; 
                    }
                }
            }
        }
        public List<ReporteVentaItem> ObtenerReporte(DateTime inicio, DateTime fin, int? usuarioId = null)
        {
            var reporte = new List<ReporteVentaItem>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT v.VentaId, v.Fecha, u.NombreUsuario, p.Nombre as Producto, d.Cantidad, d.PrecioUnitario, d.Subtotal
                    FROM Ventas v
                    INNER JOIN Usuarios u ON v.UsuarioId = u.UsuarioId
                    INNER JOIN DetalleVentas d ON v.VentaId = d.VentaId
                    INNER JOIN Productos p ON d.ProductoId = p.ProductoId
                    WHERE v.Fecha BETWEEN @Inicio AND @Fin";

                if (usuarioId.HasValue)
                {
                    query += " AND v.UsuarioId = @UserId";
                }

                query += " ORDER BY v.Fecha DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Inicio", inicio);
                    command.Parameters.AddWithValue("Fin", fin);
                    if (usuarioId.HasValue)
                    {
                        command.Parameters.AddWithValue("UserId", usuarioId.Value);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reporte.Add(new ReporteVentaItem
                            {
                                VentaId = (int)reader["VentaId"],
                                Fecha = (DateTime)reader["Fecha"],
                                Usuario = reader["NombreUsuario"].ToString(),
                                Producto = reader["Producto"].ToString(),
                                Cantidad = (decimal)reader["Cantidad"],
                                Precio = (decimal)reader["PrecioUnitario"],
                                Subtotal = (decimal)reader["Subtotal"]
                            });
                        }
                    }
                }
            }
            return reporte;
        }
    }
}
