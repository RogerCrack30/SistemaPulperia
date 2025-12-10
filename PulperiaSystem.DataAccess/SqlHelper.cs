using System;
using Npgsql;

namespace PulperiaSystem.DataAccess
{
    public static class SqlHelper
    {
        // Default for Dev (Postgres format)
        // Adjust Port 5432 and default "postgres" user if needed for local dev
        public static string ConnectionString { get; set; } = "Host=localhost;Port=5432;Database=PulperiaDB;Username=postgres;Password=root";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }
    }
}
