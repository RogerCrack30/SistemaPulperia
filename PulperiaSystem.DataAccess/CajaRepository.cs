using System;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class CajaRepository
    {
        // Verifica si el usuario tiene una caja abierta
        public CajaSesion VerificarCajaAbierta(int usuarioId)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM CajasSesiones WHERE UsuarioId = @Uid AND Estado = true LIMIT 1"; // Fixed TOP 1 and bit->bool
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Uid", usuarioId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapSesion(reader);
                        }
                    }
                }
            }
            return null;
        }

        public void AbrirCaja(int usuarioId, decimal montoInicial)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO CajasSesiones (UsuarioId, FechaInicio, MontoInicial, Estado)
                    VALUES (@Uid, CURRENT_TIMESTAMP, @Monto, true)"; // Fixed GETDATE
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Uid", usuarioId);
                    command.Parameters.AddWithValue("Monto", montoInicial);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void CerrarCaja(int sesionId, decimal esperado, decimal real, decimal diferencia)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE CajasSesiones 
                    SET FechaFin = CURRENT_TIMESTAMP, 
                        MontoFinalEsperado = @Esperado, 
                        MontoFinalReal = @Real, 
                        Diferencia = @Diff, 
                        Estado = false
                    WHERE SesionId = @Id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Esperado", esperado);
                    command.Parameters.AddWithValue("Real", real);
                    command.Parameters.AddWithValue("Diff", diferencia);
                    command.Parameters.AddWithValue("Id", sesionId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public decimal ObtenerTotalVentasEfectivo(int usuarioId, DateTime fechaInicio)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                // Asumimos que todas las ventas son efectivo ('FormaPago'='Efectivo') o sumamos todo por ahora
                string query = "SELECT COALESCE(SUM(Total), 0) FROM Ventas WHERE UsuarioId = @Uid AND Fecha >= @FechaInicio"; // Fixed ISNULL
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Uid", usuarioId);
                    command.Parameters.AddWithValue("FechaInicio", fechaInicio);
                    return (decimal)command.ExecuteScalar();
                }
            }
        }

        private CajaSesion MapSesion(NpgsqlDataReader reader)
        {
            return new CajaSesion
            {
                SesionId = (int)reader["SesionId"],
                UsuarioId = (int)reader["UsuarioId"],
                FechaInicio = (DateTime)reader["FechaInicio"],
                MontoInicial = (decimal)reader["MontoInicial"],
                Estado = (bool)reader["Estado"]
            };
        }
    }
}
