using System;
using System.Collections.Generic;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class ProveedorRepository
    {
        public List<Proveedor> ObtenerTodos()
        {
            var lista = new List<Proveedor>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Proveedores ORDER BY Nombre";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Proveedor
                            {
                                ProveedorId = (int)reader["ProveedorId"],
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

        public void Agregar(Proveedor proveedor)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Proveedores (Nombre, Telefono, Direccion) VALUES (@Nombre, @Tel, @Dir)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Nombre", proveedor.Nombre);
                    command.Parameters.AddWithValue("Tel", (object)proveedor.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("Dir", (object)proveedor.Direccion ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void Actualizar(Proveedor proveedor)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Proveedores SET Nombre=@Nombre, Telefono=@Tel, Direccion=@Dir WHERE ProveedorId=@Id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Nombre", proveedor.Nombre);
                    command.Parameters.AddWithValue("Tel", (object)proveedor.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("Dir", (object)proveedor.Direccion ?? DBNull.Value);
                    command.Parameters.AddWithValue("Id", proveedor.ProveedorId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Eliminar(int id)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Proveedores WHERE ProveedorId=@Id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
