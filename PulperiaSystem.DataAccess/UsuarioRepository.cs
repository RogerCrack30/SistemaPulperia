using System;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class UsuarioRepository
    {
        public Usuario Login(string username, string password)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                // NOTA: En un sistema real, aquí compararíamos hashes (BCrypt, SHA256, etc).
                // Como el script SQL insertó 'admin123' directo, haremos una comparación simple por ahora
                // para cumplir el flujo, pero idealmente la BD tendría el hash real.
                string query = @"
                    SELECT UsuarioId, NombreUsuario, RolId, Estado 
                    FROM Usuarios 
                    WHERE NombreUsuario = @User AND ContrasenaHash = @Pass AND Estado = true"; // POSTGRES: 1 -> true

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("User", username);
                    command.Parameters.AddWithValue("Pass", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                UsuarioId = (int)reader["UsuarioId"],
                                NombreUsuario = (string)reader["NombreUsuario"],
                                RolId = (int)reader["RolId"],
                                Estado = (bool)reader["Estado"],
                                // No devolvemos el password por seguridad
                            };
                        }
                    }
                }
            }
            return null;
        }

        public string ObtenerNombreRol(int rolId)
        {
             using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT Nombre FROM Roles WHERE RolId = @Id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Id", rolId);
                    var result = command.ExecuteScalar();
                    return result != null ? result.ToString() : "Desconocido";
                }
            }
        }

        public System.Collections.Generic.List<Usuario> ObtenerTodos()
        {
            var lista = new System.Collections.Generic.List<Usuario>();
             using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT UsuarioId, NombreUsuario, RolId, Estado FROM Usuarios";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            lista.Add(new Usuario{
                                UsuarioId = (int)reader["UsuarioId"],
                                NombreUsuario = reader["NombreUsuario"].ToString(),
                                RolId = (int)reader["RolId"],
                                Estado = (bool)reader["Estado"]
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public void Agregar(Usuario usuario, string password)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Usuarios (NombreUsuario, ContrasenaHash, RolId, Estado) VALUES (@Nombre, @Pass, @Rol, true)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Nombre", usuario.NombreUsuario);
                    command.Parameters.AddWithValue("Pass", password); // En producción usar Hash
                    command.Parameters.AddWithValue("Rol", usuario.RolId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Actualizar(Usuario usuario, string password = null)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Usuarios SET NombreUsuario = @Nombre, RolId = @Rol, Estado = @Estado WHERE UsuarioId = @Id";
                
                if (!string.IsNullOrEmpty(password))
                {
                    query = "UPDATE Usuarios SET NombreUsuario = @Nombre, RolId = @Rol, Estado = @Estado, ContrasenaHash = @Pass WHERE UsuarioId = @Id";
                }

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Nombre", usuario.NombreUsuario);
                    command.Parameters.AddWithValue("Rol", usuario.RolId);
                    command.Parameters.AddWithValue("Estado", usuario.Estado);
                    command.Parameters.AddWithValue("Id", usuario.UsuarioId);
                    if (!string.IsNullOrEmpty(password)) command.Parameters.AddWithValue("Pass", password);
                    
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
