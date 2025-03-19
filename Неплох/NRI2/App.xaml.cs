using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.Classes;
using NRI.ViewModel;
using NRI;
using System;
using System.IO;
using System.Windows;

namespace NRI
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Конфигурация приложения
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Строка подключения не найдена в конфигурации.");
            }

            // Настройка DI-контейнера
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Показываем окно Авторизации первым
            var registrationWindow = ServiceProvider.GetRequiredService<Autorizatsaya>();
            registrationWindow.Show();
        }
        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Регистрация конфигурации
            services.AddSingleton(configuration);

            // Регистрация базы данных
            services.AddTransient<IDatabaseService, DatabaseService>(provider =>
                new DatabaseService(configuration.GetConnectionString("DefaultConnection")));

            // Регистрация репозиториев
            services.AddTransient<IUserRepository, UserRepository>();

            // Регистрация сервисов
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<INavigationService, NavigationService>();

            // Регистрация контекста БД (Transient для EF Core)
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Transient);
            services.AddDbContext<EventContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Transient);

            // Регистрация ViewModels
            services.AddTransient<EventsViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<DiceRollerViewModel>();

            // Регистрация окон
            services.AddTransient<Autorizatsaya>();
            services.AddTransient<MainWindow>();
            services.AddTransient<Player>();
            services.AddTransient<staff>();
            services.AddTransient<Reviews>();
            services.AddTransient<Settings>();
            services.AddTransient<DiceRoller>();
            services.AddTransient<Events>();


            services.AddLogging(configure => configure.AddConsole());
            // Настройка логирования
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }
    }
}