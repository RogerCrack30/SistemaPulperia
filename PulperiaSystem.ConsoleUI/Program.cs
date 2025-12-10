using System;
using PulperiaSystem.DataAccess;
using PulperiaSystem.Models;

namespace PulperiaSystem.ConsoleUI
{
    class Program
    {
        static Usuario _usuarioActual;
        static UsuarioRepository _usuarioRepo = new UsuarioRepository();

        static void Main(string[] args)
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
            Console.WriteLine("========================================");
            Console.WriteLine("      SISTEMA DE PULPERIA - .NET 8      ");
            Console.WriteLine("========================================");

            if (RealizarLogin())
            {
                MostrarMenuPrincipal();
            }
            else
            {
                Console.WriteLine("\nSaliendo del sistema...");
            }
        }

        static bool RealizarLogin()
        {
            int intentos = 0;
            while (intentos < 3)
            {
                Console.Write("\nUsuario: ");
                string user = Console.ReadLine();
                Console.Write("Contraseña: ");
                string pass = LeerPassword();

                var usuario = _usuarioRepo.Login(user, pass);

                if (usuario != null)
                {
                    _usuarioActual = usuario;
                    string nombreRol = _usuarioRepo.ObtenerNombreRol(usuario.RolId);
                    Console.WriteLine($"\n¡Bienvenido {usuario.NombreUsuario}! (Rol: {nombreRol})");
                    return true;
                }
                else
                {
                    Console.WriteLine("\nCredenciales incorrectas o usuario inactivo.");
                    intentos++;
                    Console.WriteLine($"Intentos restantes: {3 - intentos}");
                }
            }
            return false;
        }

        static string LeerPassword()
        {
            if (Console.IsInputRedirected)
            {
                // Fallback para pruebas automatizadas
                return Console.ReadLine();
            }

            string pass = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                // Ignorar teclas de control que no sean Backspace o Enter
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, (pass.Length - 1));
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            return pass;
        }

        static void MostrarMenuPrincipal()
        {
            string opcion = "";
            do
            {
                if (!Console.IsOutputRedirected) Console.Clear();
                Console.WriteLine("\n--- MENU PRINCIPAL ---");
                Console.WriteLine("1. Ventas");
                Console.WriteLine("2. Compras");
                Console.WriteLine("3. Inventario (Productos)");
                Console.WriteLine("4. Personas (Clientes/Proveedores)");
                Console.WriteLine("5. Reportes");
                Console.WriteLine("0. Salir");
                Console.Write("Seleccione una opción: ");
                
                opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        MenuVentas();
                        break;
                    case "2":
                        MenuCompras();
                        break;
                    case "3":
                        MenuInventario();
                        break;
                    case "4":
                        MenuPersonas();
                        break;
                    case "5":
                        MenuReportes();
                        break;
                    case "0":
                        Console.WriteLine("Saliendo...");
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
                if (opcion != "0")
                {
                    Console.WriteLine("Presione ENTER para continuar...");
                    Console.ReadLine();
                }

            } while (opcion != "0");
        }

        static void MenuVentas()
        {
            var ventaRepo = new VentaRepository();
            var prodRepo = new ProductoRepository();
            var carrito = new List<DetalleVenta>();
            var productosCache = prodRepo.ObtenerTodos();

            while (true)
            {
                if (!Console.IsOutputRedirected) Console.Clear();
                Console.WriteLine("\n--- NUEVA VENTA ---");
                Console.WriteLine($"Items en carrito: {carrito.Count}");
                decimal totalActual = 0;
                foreach(var i in carrito) totalActual += i.Subtotal;
                Console.WriteLine($"Total Acumulado: {totalActual:C2}");
                
                Console.WriteLine("\n1. Agregar Producto");
                Console.WriteLine("2. Finalizar Venta");
                Console.WriteLine("3. Cancelar / Volver");
                Console.Write("Opción: ");
                string op = Console.ReadLine();

                if (op == "3") break;

                if (op == "1")
                {
                    Console.WriteLine("\n--- PRODUCTOS DISPONIBLES ---");
                    foreach(var p in productosCache) 
                    {
                         if(p.StockActual > 0)
                            Console.WriteLine($"{p.ProductoId}. {p.Nombre} ({p.PrecioVenta:C2}) [Stock: {p.StockActual}]");
                    }
                    
                    Console.Write("ID Producto: ");
                    if(int.TryParse(Console.ReadLine(), out int pid))
                    {
                        var prod = productosCache.Find(p => p.ProductoId == pid);
                        if (prod != null)
                        {
                            Console.Write("Cantidad: ");
                            if(decimal.TryParse(Console.ReadLine(), out decimal cant))
                            {
                                if (cant <= prod.StockActual)
                                {
                                    var item = new DetalleVenta
                                    {
                                        ProductoId = pid,
                                        Cantidad = cant,
                                        PrecioUnitario = prod.PrecioVenta,
                                        Subtotal = cant * prod.PrecioVenta
                                    };
                                    carrito.Add(item);
                                    prod.StockActual -= cant; 
                                }
                                else { Console.WriteLine("Stock insuficiente."); Console.ReadLine(); }
                            }
                        }
                    }
                }
                else if (op == "2")
                {
                    if (carrito.Count == 0) { Console.WriteLine("Carrito vacío."); Console.ReadLine(); continue; }
                    
                    Console.Write("ID Cliente (Enter para genérico/NULL): ");
                    string cliInput = Console.ReadLine();
                    int? clienteId = string.IsNullOrEmpty(cliInput) ? (int?)null : int.Parse(cliInput);

                    var venta = new Venta
                    {
                        UsuarioId = _usuarioActual.UsuarioId,
                        ClienteId = clienteId,
                        Total = totalActual,
                        FormaPago = "Efectivo",
                        Fecha = DateTime.Now
                    };

                    try
                    {
                        ventaRepo.RegistrarVenta(venta, carrito);
                        Console.WriteLine("¡Venta Registrada Exitosamente!");
                        Console.ReadLine();
                        break;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Error al procesar venta: {ex.Message}");
                        Console.ReadLine();
                    }
                }
            }
        }

        static void MenuCompras()
        {
            var compraRepo = new CompraRepository();
            var prodRepo = new ProductoRepository();
            var provRepo = new ProveedorRepository();
            var carrito = new List<DetalleCompra>();
            var productosCache = prodRepo.ObtenerTodos();

            while (true)
            {
                if (!Console.IsOutputRedirected) Console.Clear();
                Console.WriteLine("\n--- NUEVA COMPRA ---");
                Console.WriteLine($"Items en carrito: {carrito.Count}");
                decimal totalActual = 0;
                foreach(var i in carrito) totalActual += i.Subtotal;
                Console.WriteLine($"Total Acumulado: {totalActual:C2}");
                
                Console.WriteLine("\n1. Agregar Producto");
                Console.WriteLine("2. Finalizar Compra");
                Console.WriteLine("3. Cancelar / Volver");
                Console.Write("Opción: ");
                string op = Console.ReadLine();

                if (op == "3") break;

                if (op == "1")
                {
                    Console.WriteLine("\n--- PRODUCTOS EXISTENTES ---");
                    foreach(var p in productosCache) 
                    {
                        Console.WriteLine($"{p.ProductoId}. {p.Nombre} (Costo Ref: {p.PrecioCompra:C2})");
                    }
                    
                    Console.Write("ID Producto: ");
                    if(int.TryParse(Console.ReadLine(), out int pid))
                    {
                        var prod = productosCache.Find(p => p.ProductoId == pid);
                        if (prod != null)
                        {
                            Console.Write("Cantidad Comprada: ");
                            decimal.TryParse(Console.ReadLine(), out decimal cant);

                            Console.Write("Costo Unitario (Enter para usar referencia): ");
                            string costoInput = Console.ReadLine();
                            decimal costo = string.IsNullOrEmpty(costoInput) ? prod.PrecioCompra : decimal.Parse(costoInput);

                            var item = new DetalleCompra
                            {
                                ProductoId = pid,
                                Cantidad = cant,
                                PrecioUnitario = costo,
                                Subtotal = cant * costo
                            };
                            carrito.Add(item);
                        }
                    }
                }
                else if (op == "2")
                {
                    if (carrito.Count == 0) { Console.WriteLine("Carrito vacío."); Console.ReadLine(); continue; }
                    
                    Console.Write("ID Proveedor: ");
                    if(int.TryParse(Console.ReadLine(), out int provId))
                    {
                        var compra = new Compra
                        {
                            UsuarioId = _usuarioActual.UsuarioId,
                            ProveedorId = provId,
                            Total = totalActual,
                            Fecha = DateTime.Now
                        };

                        try
                        {
                            compraRepo.RegistrarCompra(compra, carrito);
                            Console.WriteLine("¡Compra Registrada Exitosamente!");
                            Console.ReadLine();
                            break;
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Error al procesar compra: {ex.Message}");
                            Console.ReadLine();
                        }
                    }
                }
            }
        }

        static void MenuReportes()
        {
            var reporteRepo = new ReporteRepository();
            string opcion = "";

            do
            {
                if (!Console.IsOutputRedirected) Console.Clear();
                Console.WriteLine("\n--- REPORTES ---");
                Console.WriteLine("1. Ventas del Día");
                Console.WriteLine("2. Productos con Bajo Stock");
                Console.WriteLine("0. Volver");
                Console.Write("Seleccione una opción: ");
                opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        var ventasHoy = reporteRepo.ObtenerVentasDelDia();
                        Console.WriteLine("\n--- VENTAS DE HOY ---");
                        if (ventasHoy.Count == 0)
                        {
                            Console.WriteLine("No hay ventas registradas hoy.");
                        }
                        else
                        {
                            Console.WriteLine($"{"Hora",-10} {"Producto",-20} {"Cant.",-8} {"Precio",-10} {"Subtotal",-10}");
                            decimal granTotal = 0;
                            foreach (var v in ventasHoy)
                            {
                                Console.WriteLine($"{v.Fecha.ToString("HH:mm:ss"),-10} {v.Producto,-20} {v.Cantidad,-8} {v.Precio,-10:C} {v.Subtotal,-10:C}");
                                granTotal += v.Subtotal;
                            }
                            Console.WriteLine("-------------------------------------------------------------");
                            Console.WriteLine($"TOTAL DEL DÍA: {granTotal:C}");
                        }
                        break;

                    case "2":
                        var bajoStock = reporteRepo.ObtenerProductosBajosStock();
                        Console.WriteLine("\n--- PRODUCTOS BAJO STOCK ---");
                        if (bajoStock.Count == 0)
                        {
                            Console.WriteLine("Todos los productos tienen niveles saludables de stock.");
                        }
                        else
                        {
                            Console.WriteLine($"{"ID",-5} {"Nombre",-20} {"Stock",-10} {"Mínimo",-10}");
                            foreach (var p in bajoStock)
                            {
                                Console.WriteLine($"{p.ProductoId,-5} {p.Nombre,-20} {p.StockActual,-10} {p.StockMinimo,-10}");
                            }
                        }
                        break;

                    case "0":
                        break;

                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }

                if (opcion != "0")
                {
                    Console.WriteLine("Presione ENTER para continuar...");
                    Console.ReadLine();
                }

            } while (opcion != "0");
        }

        static void MenuPersonas()
        {
            string opcion = "";
            var clienteRepo = new ClienteRepository();
            var provRepo = new ProveedorRepository();

            do
            {
                if (!Console.IsOutputRedirected) Console.Clear();
                Console.WriteLine("\n--- GESTION DE PERSONAS ---");
                Console.WriteLine("1. Listar Clientes");
                Console.WriteLine("2. Agregar Cliente");
                Console.WriteLine("3. Listar Proveedores");
                Console.WriteLine("4. Agregar Proveedor");
                Console.WriteLine("0. Volver");
                Console.Write("Seleccione una opción: ");
                opcion = Console.ReadLine();

                switch(opcion)
                {
                    case "1":
                        var clientes = clienteRepo.ObtenerTodos();
                        Console.WriteLine("\n--- LISTA DE CLIENTES ---");
                        Console.WriteLine($"{"ID",-5} {"Nombre",-25} {"Teléfono",-15}");
                        foreach(var c in clientes) Console.WriteLine($"{c.ClienteId,-5} {c.Nombre,-25} {c.Telefono,-15}");
                        break;
                    case "2":
                        try
                        {
                            Console.WriteLine("\n--- NUEVO CLIENTE ---");
                            var c = new Cliente();
                            Console.Write("Nombre: "); c.Nombre = Console.ReadLine();
                            Console.Write("Teléfono: "); c.Telefono = Console.ReadLine();
                            Console.Write("Dirección: "); c.Direccion = Console.ReadLine();
                            clienteRepo.Agregar(c);
                            Console.WriteLine("¡Cliente agregado!");
                        }
                        catch(Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
                        break;
                    case "3":
                        var proovedores = provRepo.ObtenerTodos();
                        Console.WriteLine("\n--- LISTA DE PROVEEDORES ---");
                        Console.WriteLine($"{"ID",-5} {"Nombre",-25} {"Teléfono",-15}");
                        foreach(var p in proovedores) Console.WriteLine($"{p.ProveedorId,-5} {p.Nombre,-25} {p.Telefono,-15}");
                        break;
                    case "4":
                        try
                        {
                            Console.WriteLine("\n--- NUEVO PROVEEDOR ---");
                            var p = new Proveedor();
                            Console.Write("Nombre: "); p.Nombre = Console.ReadLine();
                            Console.Write("Teléfono: "); p.Telefono = Console.ReadLine();
                            Console.Write("Dirección: "); p.Direccion = Console.ReadLine();
                            provRepo.Agregar(p);
                            Console.WriteLine("¡Proveedor agregado!");
                        }
                        catch(Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
                        break;
                }
                
                if (opcion != "0" && opcion != "")
                {
                     Console.WriteLine("Presione ENTER para continuar...");
                     Console.ReadLine();
                }

            } while (opcion != "0");
        }

        static void MenuInventario()
        {
            string opcion = "";
            var prodRepo = new ProductoRepository();
            var catRepo = new CategoriaRepository();

            do
            {
                if (!Console.IsOutputRedirected) Console.Clear();
                Console.WriteLine("\n--- GESTION DE INVENTARIO ---");
                Console.WriteLine("1. Listar Productos");
                Console.WriteLine("2. Agregar Producto");
                Console.WriteLine("0. Volver");
                Console.Write("Seleccione una opción: ");
                opcion = Console.ReadLine();

                switch(opcion)
                {
                    case "1":
                        var productos = prodRepo.ObtenerTodos();
                        Console.WriteLine("\n--- LISTA DE PRODUCTOS ---");
                        Console.WriteLine($"{"ID",-5} {"Nombre",-20} {"Precio V.",-10} {"Stock",-10}");
                        foreach(var p in productos)
                        {
                            Console.WriteLine($"{p.ProductoId,-5} {p.Nombre,-20} {p.PrecioVenta,-10:C} {p.StockActual,-10}");
                        }
                        break;
                    case "2":
                        try
                        {
                            Console.WriteLine("\n--- NUEVO PRODUCTO ---");
                            var p = new Producto();
                            
                            Console.Write("Código: ");
                            p.Codigo = Console.ReadLine();

                            Console.Write("Nombre: ");
                            p.Nombre = Console.ReadLine();

                            // Listar categorías para seleccionar
                            var categorias = catRepo.ObtenerTodas();
                            Console.WriteLine("Categorías disponibles:");
                            foreach(var c in categorias) Console.WriteLine($"{c.CategoriaId}. {c.Nombre}");
                            Console.Write("ID Categoría: ");
                            p.CategoriaId = int.Parse(Console.ReadLine());

                            Console.Write("Precio Compra: ");
                            p.PrecioCompra = decimal.Parse(Console.ReadLine());

                            Console.Write("Precio Venta: ");
                            p.PrecioVenta = decimal.Parse(Console.ReadLine());

                            Console.Write("Stock Inicial: ");
                            p.StockActual = decimal.Parse(Console.ReadLine());

                            Console.Write("Stock Mínimo: ");
                            p.StockMinimo = decimal.Parse(Console.ReadLine());

                            Console.Write("Unidad Medida (ej. UN, KG): ");
                            p.UnidadMedida = Console.ReadLine();

                            prodRepo.Agregar(p);
                            Console.WriteLine("¡Producto agregado exitosamente!");
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Error al agregar producto: {ex.Message}");
                        }
                        break;
                    case "0":
                        break;
                    default:
                         Console.WriteLine("Opción inválida.");
                         break;
                }
                
                if (opcion != "0")
                {
                     Console.WriteLine("Presione ENTER para continuar...");
                     Console.ReadLine();
                }

            } while (opcion != "0");
        }
    }
}
