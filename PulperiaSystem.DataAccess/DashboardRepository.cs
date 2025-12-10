using System;
using System.Collections.Generic;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class DashboardRepository
    {
        public decimal ObtenerTotalVentasHoy()
        {
            using (var conn = SqlHelper.GetConnection())
            {
                conn.Open();
                // SQL Server: CAST(GETDATE() AS DATE)
                // Postgres: CURRENT_DATE
                string query = "SELECT COALESCE(SUM(Total), 0) FROM Ventas WHERE CAST(Fecha AS DATE) = CURRENT_DATE";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    return Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
        }

        public int ObtenerCantidadVentasHoy()
        {
            using (var conn = SqlHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Ventas WHERE CAST(Fecha AS DATE) = CURRENT_DATE";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public List<Producto> ObtenerProductosBajoStock(int limite = 10)
        {
            var lista = new List<Producto>();
            using (var conn = SqlHelper.GetConnection())
            {
                conn.Open();
                // SQL Server: TOP (@Limite) ...
                // Postgres: ... LIMIT @Limite
                string query = "SELECT ProductoId, Codigo, Nombre, StockActual, StockMinimo FROM Productos WHERE StockActual <= StockMinimo ORDER BY StockActual ASC LIMIT @Limite";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("Limite", limite);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Producto
                            {
                                ProductoId = (int)reader["ProductoId"],
                                Codigo = reader["Codigo"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                StockActual = Convert.ToDecimal(reader["StockActual"]),
                                StockMinimo = Convert.ToDecimal(reader["StockMinimo"])
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}
