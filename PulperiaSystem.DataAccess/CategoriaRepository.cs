using System;
using System.Collections.Generic;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class CategoriaRepository
    {
        public List<Categoria> ObtenerTodas()
        {
            var lista = new List<Categoria>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT CategoriaId, Nombre FROM Categorias";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Categoria
                            {
                                CategoriaId = (int)reader["CategoriaId"],
                                Nombre = (string)reader["Nombre"]
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}
