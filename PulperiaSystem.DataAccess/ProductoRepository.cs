using System;
using System.Collections.Generic;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class ProductoRepository
    {
        public List<Producto> ObtenerTodos()
        {
            var lista = new List<Producto>();
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Productos ORDER BY Nombre";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapProducto(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public void Agregar(Producto producto)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO Productos (Codigo, Nombre, CategoriaId, PrecioCompra, PrecioVenta, StockActual, StockMinimo, UnidadMedida, Estado)
                    VALUES (@Codigo, @Nombre, @CatId, @PCompra, @PVenta, @Stock, @MinStock, @Unidad, true)"; // POSTGRES True
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Codigo", producto.Codigo);
                    command.Parameters.AddWithValue("Nombre", producto.Nombre);
                    command.Parameters.AddWithValue("CatId", producto.CategoriaId);
                    command.Parameters.AddWithValue("PCompra", producto.PrecioCompra);
                    command.Parameters.AddWithValue("PVenta", producto.PrecioVenta);
                    command.Parameters.AddWithValue("Stock", producto.StockActual);
                    command.Parameters.AddWithValue("MinStock", producto.StockMinimo);
                    command.Parameters.AddWithValue("Unidad", producto.UnidadMedida);

                    command.ExecuteNonQuery();
                }
            }
        }

        private Producto MapProducto(NpgsqlDataReader reader)
        {
            return new Producto
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
            };
        }
        public void Actualizar(Producto producto)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE Productos 
                    SET Codigo=@Codigo, Nombre=@Nombre, CategoriaId=@CatId, 
                        PrecioCompra=@PCompra, PrecioVenta=@PVenta, 
                        StockActual=@Stock, StockMinimo=@MinStock, UnidadMedida=@Unidad, Estado=@Estado
                    WHERE ProductoId=@Id";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Codigo", producto.Codigo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("Nombre", producto.Nombre);
                    command.Parameters.AddWithValue("CatId", producto.CategoriaId);
                    command.Parameters.AddWithValue("PCompra", producto.PrecioCompra);
                    command.Parameters.AddWithValue("PVenta", producto.PrecioVenta);
                    command.Parameters.AddWithValue("Stock", producto.StockActual);
                    command.Parameters.AddWithValue("MinStock", producto.StockMinimo);
                    command.Parameters.AddWithValue("Unidad", producto.UnidadMedida ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("Estado", producto.Estado);
                    command.Parameters.AddWithValue("Id", producto.ProductoId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Eliminar(int productoId)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Productos SET Estado = false WHERE ProductoId = @Id"; 
                // Fisico test:
                query = "DELETE FROM Productos WHERE ProductoId = @Id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Id", productoId);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void AjustarStock(int productoId, decimal cantidadAjuste)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Productos SET StockActual = StockActual + @Cant WHERE ProductoId = @Id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Cant", cantidadAjuste);
                    command.Parameters.AddWithValue("Id", productoId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
