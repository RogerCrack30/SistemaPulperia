using System;
using System.Collections.Generic;
using Npgsql;

namespace PulperiaSystem.DataAccess
{
    public class ConfiguracionRepository
    {
        public string ObtenerValor(string clave)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT Valor FROM Configuracion WHERE Clave = @Clave";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Clave", clave);
                    var result = command.ExecuteScalar();
                    return result?.ToString() ?? "";
                }
            }
        }

        public Dictionary<string, string> ObtenerConfiguracionCompleta()
        {
            var config = new Dictionary<string, string>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT Clave, Valor FROM Configuracion";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            config[reader["Clave"].ToString()] = reader["Valor"].ToString();
                        }
                    }
                }
            }
            return config;
        }
    }
}
