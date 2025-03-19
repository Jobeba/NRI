using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NRI.Classes;
using System;
using System.IO;
using System.Windows;

namespace NRI
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Чтение конфигурации из appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Получение строки подключения
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Настройка DI-контейнера
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, connectionString);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Отображение главного окна
            var Registrasya = _serviceProvider.GetRequiredService<Registrasya>();
            Registrasya.Show();
        }

        private void ConfigureServices(IServiceCollection services, string connectionString)
        {
            // Регистрация сервисов
            services.AddTransient<IDatabaseService, DatabaseService>(provider =>
                new DatabaseService(connectionString));

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<Registrasya>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }
    }
}