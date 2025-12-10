using System;
using System.Collections.Generic;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class ClienteRepository
    {
        public List<Cliente> ObtenerTodos()
        {
            var lista = new List<Cliente>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Clientes ORDER BY Nombre";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Cliente
                            {
                                ClienteId = (int)reader["ClienteId"],
                                Nombre = (string)reader["Nombre"],
                                Telefono = reader["Telefono"] as string,
                                Direccion = reader["Direccion"] as string
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public void Agregar(Cliente cliente)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Clientes (Nombre, Telefono, Direccion) VALUES (@Nombre, @Tel, @Dir)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("Tel", (object)cliente.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("Dir", (object)cliente.Direccion ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void Actualizar(Cliente cliente)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Clientes SET Nombre=@Nombre, Telefono=@Tel, Direccion=@Dir WHERE ClienteId=@Id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("Tel", (object)cliente.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("Dir", (object)cliente.Direccion ?? DBNull.Value);
                    command.Parameters.AddWithValue("Id", cliente.ClienteId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Eliminar(int id)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Clientes WHERE ClienteId=@Id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
