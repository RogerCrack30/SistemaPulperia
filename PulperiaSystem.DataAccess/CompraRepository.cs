using System;
using System.Collections.Generic;
using Npgsql;
using PulperiaSystem.Models;

namespace PulperiaSystem.DataAccess
{
    public class CompraRepository
    {
        public bool RegistrarCompra(Compra compra, List<DetalleCompra> detalles)
        {
            using (var connection = SqlHelper.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar Encabezado de Compra
                        // POSTGRES: OUTPUT INSERTED.CompraId => RETURNING CompraId
                        string queryCompra = @"
                            INSERT INTO Compras (Fecha, ProveedorId, UsuarioId, Total) 
                            VALUES (@Fecha, @ProvId, @UserId, @Total)
                            RETURNING CompraId";

                        int compraId;
                        using (var cmdCompra = new NpgsqlCommand(queryCompra, connection, transaction))
                        {
                            cmdCompra.Parameters.AddWithValue("Fecha", DateTime.Now);
                            cmdCompra.Parameters.AddWithValue("ProvId", compra.ProveedorId);
                            cmdCompra.Parameters.AddWithValue("UserId", compra.UsuarioId);
                            cmdCompra.Parameters.AddWithValue("Total", compra.Total);
                            
                            compraId = (int)cmdCompra.ExecuteScalar();
                        }

                        // 2. Insertar Detalles y AUMENTAR Stock
                        foreach (var det in detalles)
                        {
                            // A. Insertar Detalle
                            string queryDetalle = @"
                                INSERT INTO DetalleCompras (CompraId, ProductoId, Cantidad, PrecioUnitario, Subtotal)
                                VALUES (@CompraId, @ProdId, @Cant, @Prec, @Sub)";
                            
                            using (var cmdDetalle = new NpgsqlCommand(queryDetalle, connection, transaction))
                            {
                                cmdDetalle.Parameters.AddWithValue("CompraId", compraId);
                                cmdDetalle.Parameters.AddWithValue("ProdId", det.ProductoId);
                                cmdDetalle.Parameters.AddWithValue("Cant", det.Cantidad);
                                cmdDetalle.Parameters.AddWithValue("Prec", det.PrecioUnitario);
                                cmdDetalle.Parameters.AddWithValue("Sub", det.Subtotal);
                                cmdDetalle.ExecuteNonQuery();
                            }

                            // B. Actualizar Stock (SUMAR)
                            string queryStock = "UPDATE Productos SET StockActual = StockActual + @Cant WHERE ProductoId = @ProdId";
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
    }
}
