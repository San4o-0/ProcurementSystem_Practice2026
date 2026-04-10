using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Data;
using ProcurementSystem.Services;
using System;
using System.Windows;

namespace ProcurementSystem.Wpf
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, exArgs) =>
            {
                MessageBox.Show($"КРИТИЧНА ПОМИЛКА:\n{exArgs.Exception.Message}\n\n{exArgs.Exception.StackTrace}");
                exArgs.Handled = true; // ✅ НЕ крашить застосунок
            };
            base.OnStartup(e);
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddScoped<AuditService>();
            services.AddScoped<PurchaseRequestService>();


            services.AddScoped<AuthService>();
            services.AddScoped<UserService>();

            Services = services.BuildServiceProvider();

            // 🔑 Ініціалізація БД
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DbInitializer.EnsureAdmin(db);

            // 🚀 ПОКАЗУЄМО LOGIN
            var loginWindow = new LoginWindow();
            loginWindow.Show();

        }
    }
}
