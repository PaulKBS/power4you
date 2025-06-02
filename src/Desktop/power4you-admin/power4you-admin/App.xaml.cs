using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using power4you_admin.Data;
using power4you_admin.Services;
using power4you_admin.Views;

namespace power4you_admin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        
        public IServiceProvider ServiceProvider => _host?.Services ?? throw new InvalidOperationException("Service provider not available");

        protected override void OnStartup(StartupEventArgs e)
        {
            // Configure services
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Configure Entity Framework with MySQL using DbContextFactory
                    services.AddDbContextFactory<SolarDbContext>(options =>
                        options.UseMySql(
                            "Server=w012ac34.kasserver.com;Port=3306;Database=d03a150f;Uid=d03a150f;Pwd=Tp678CWc859CX4Xg;",
                            ServerVersion.AutoDetect("Server=w012ac34.kasserver.com;Port=3306;Database=d03a150f;Uid=d03a150f;Pwd=Tp678CWc859CX4Xg;")
                        ));

                    // Register services
                    services.AddScoped<DatabaseService>();
                    services.AddScoped<AnlageService>();
                    services.AddScoped<SolarmodultypService>();
                    services.AddScoped<CustomerService>();
                    
                    // Register pages and dialogs
                    services.AddTransient<CustomersPage>();
                    services.AddTransient<CustomerEditDialog>();
                    services.AddTransient<AnlagenPage>();
                    services.AddTransient<AnlageEditDialog>();
                    services.AddTransient<AnlageCreateDialog>();
                    
                    // Register windows
                    services.AddTransient<MainWindow>();
                })
                .Build();

            // Start the host
            _host.Start();

            // Create and show main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}
