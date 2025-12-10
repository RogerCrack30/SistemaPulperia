using System.Windows;
using Microsoft.Extensions.Configuration;
using System.IO;
using PulperiaSystem.DataAccess;
using Application = System.Windows.Application;

namespace PulperiaSystem.UI
{
    public partial class App : Application
    {
        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var config = builder.Build();
            
            var remoteString = config.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(remoteString))
            {
                SqlHelper.ConnectionString = remoteString;
            }
        }
    }
}
