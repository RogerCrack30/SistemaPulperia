using System;
using System.Collections.Generic;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class ReporteRepository
    {
        public List<ReporteVentaItem> ObtenerVentasDelDia()
        {
            var lista = new List<ReporteVentaItem>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                // CAST(v.Fecha AS DATE) = CAST(GETDATE() AS DATE) filtra solo por la fecha de hoy ignorando hora
                // POSTGRES: CURRENT_DATE
                string query = @"
                    SELECT v.VentaId, v.Fecha, p.Nombre as Producto, dv.Cantidad, dv.PrecioUnitario, dv.Subtotal
                    FROM DetalleVentas dv
                    INNER JOIN Ventas v ON dv.VentaId = v.VentaId
                    INNER JOIN Productos p ON dv.ProductoId = p.ProductoId
                    WHERE CAST(v.Fecha AS DATE) = CURRENT_DATE
                    ORDER BY v.Fecha DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReporteVentaItem
                            {
                                VentaId = (int)reader["VentaId"],
                                Fecha = (DateTime)reader["Fecha"],
                                Producto = (string)reader["Producto"],
                                Cantidad = (decimal)reader["Cantidad"],
                                Precio = (decimal)reader["PrecioUnitario"],
                                Subtotal = (decimal)reader["Subtotal"]
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<Producto> ObtenerProductosBajosStock()
        {
            var lista = new List<Producto>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Productos WHERE StockActual <= StockMinimo ORDER BY StockActual ASC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Producto
                            {
                                ProductoId = (int)reader["ProductoId"],
                                Codigo = reader["Codigo"] as string,
                                Nombre = (string)reader["Nombre"],
                                CategoriaId = (int)reader["CategoriaId"],
                                PrecioCompra = (decimal)reader["PrecioCompra"],
                                PrecioVenta = (decimal)reader["PrecioVenta"],
                                StockActual = (decimal)reader["StockActual"],
                                StockMinimo = (decimal)reader["StockMinimo"],
                                UnidadMedida = (string)reader["UnidadMedida"],
                                Estado = (bool)reader["Estado"]
                            });
                        }
                    }
                }
            }
            return lista;
        }
        public List<ReporteVentaItem> ObtenerVentasPorRango(DateTime inicio, DateTime fin)
        {
            var lista = new List<ReporteVentaItem>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT v.VentaId, v.Fecha, p.Nombre as Producto, dv.Cantidad, dv.PrecioUnitario, dv.Subtotal
                    FROM DetalleVentas dv
                    INNER JOIN Ventas v ON dv.VentaId = v.VentaId
                    INNER JOIN Productos p ON dv.ProductoId = p.ProductoId
                    WHERE CAST(v.Fecha AS DATE) BETWEEN @Inicio AND @Fin
                    ORDER BY v.Fecha DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Inicio", inicio.Date);
                    command.Parameters.AddWithValue("Fin", fin.Date); // Fin inclusive si es DATE
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReporteVentaItem
                            {
                                VentaId = (int)reader["VentaId"],
                                Fecha = (DateTime)reader["Fecha"],
                                Producto = (string)reader["Producto"],
                                Cantidad = (decimal)reader["Cantidad"],
                                Precio = (decimal)reader["PrecioUnitario"],
                                Subtotal = (decimal)reader["Subtotal"]
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public ReporteDashboard ObtenerDashboard(DateTime inicio, DateTime fin)
        {
            var dash = new ReporteDashboard();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                
                // Ventas Totales y Transacciones
                string queryVentas = @"
                    SELECT COALESCE(SUM(Total),0) as TotalVentas, COUNT(*) as NumTrans
                    FROM Ventas
                    WHERE CAST(Fecha AS DATE) BETWEEN @Inicio AND @Fin";

                // Ganancia Estimada: (PrecioVenta - CostoActual) * CantidadVendida
                // Nota: Esto es un estimado porque usa el costo actual del producto, no el histórico.
                string queryGanancia = @"
                    SELECT COALESCE(SUM((dv.PrecioUnitario - p.PrecioCompra) * dv.Cantidad), 0)
                    FROM DetalleVentas dv
                    INNER JOIN Ventas v ON dv.VentaId = v.VentaId
                    INNER JOIN Productos p ON dv.ProductoId = p.ProductoId
                    WHERE CAST(v.Fecha AS DATE) BETWEEN @Inicio AND @Fin";

                using (var cmd = new NpgsqlCommand(queryVentas, connection))
                {
                    cmd.Parameters.AddWithValue("Inicio", inicio.Date);
                    cmd.Parameters.AddWithValue("Fin", fin.Date);
                    using(var reader = cmd.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            dash.VentasTotales = (decimal)reader["TotalVentas"];
                            dash.TotalTransacciones = (int)((long)reader["NumTrans"]); // Count is long/Int64 in Postgres
                        }
                    }
                }

                using (var cmd = new NpgsqlCommand(queryGanancia, connection))
                {
                    cmd.Parameters.AddWithValue("Inicio", inicio.Date);
                    cmd.Parameters.AddWithValue("Fin", fin.Date);
                    object result = cmd.ExecuteScalar();
                    dash.GananciaEstimada = result != null ? Convert.ToDecimal(result) : 0;
                }
            }
            return dash;
        }
    }
}
