using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.Classes;
using System;
using System.IO;
using System.Windows;
using NRI.Models;
using NRI.Classes.Email;
using NRI.Pages;
using NRI.Controls;
using NRI.DiceRoll;
using NRI.ViewModels;
using NRI.Data;
using NRI.Services;
using NRI.Windows;

namespace NRI
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                Configuration = builder.Build();

                var services = new ServiceCollection();
                ConfigureServices(services);
                ServiceProvider = services.BuildServiceProvider();

                InitializeDatabase();

                // Всегда показываем окно авторизации, MainWindow откроется сам при успешной аутентификации
                var authWindow = ServiceProvider.GetRequiredService<Autorizatsaya>();
                authWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка при запуске: {ex.Message}");
                Shutdown();
            }
        }

        private void InitializeDatabase()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate(); 
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Регистрация конфигурации
            services.Configure<DatabaseSettings>(Configuration.GetSection("ConnectionStrings"));

            // Регистрация базы данных
            services.AddTransient<IDatabaseService>(provider =>
                  new DatabaseService(
                      provider.GetRequiredService<IConfigService>(),
                      provider.GetRequiredService<ILogger<DatabaseService>>()
                  ));

            // Регистрация сервисов
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
            services.AddSingleton<ILogger<Autorizatsaya>>(provider =>
                        provider.GetRequiredService<ILoggerFactory>().CreateLogger<Autorizatsaya>());
            services.AddSingleton<ILogger<MainWindow>>(provider =>
                    provider.GetRequiredService<ILoggerFactory>().CreateLogger<MainWindow>());
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddTransient<IUserRepository>(provider =>
                  new UserRepository(
                      provider.GetRequiredService<IDatabaseService>(),
                      provider.GetRequiredService<IConfigService>(),
                      provider.GetRequiredService<ILogger<UserRepository>>()
                  ));
            services.AddSingleton<JwtService>();
            var jwtSecret = Configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                            // Генерируем новый секрет только для разработки
            #if DEBUG
                            jwtSecret = JwtService.GenerateJwtSecret();
                            Console.WriteLine($"Сгенерирован JWT Secret: {jwtSecret}");
            #else
                    throw new Exception("JWT Secret не настроен в конфигурации");
            #endif
            }

            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<JwtService>();

            // Регистрация контекста БД

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptions => sqlServerOptions.MigrationsAssembly("NRI")));

            // Регистрация ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<DiceRollerViewModel>();

            // Регистрация Controls
            services.AddTransient<AdminWindowControl>();
            services.AddTransient<OrganizerWindowControl>();
            services.AddTransient<PlayerWindowControl>();

            // Регистрация окон
            services.AddTransient<Autorizatsaya>();
            services.AddTransient<MainWindow>();
            services.AddTransient<Player>();
            services.AddTransient<staff>();
            services.AddTransient<Reviews>();
            services.AddTransient<Settings>();
            services.AddTransient<PDFPreviewWindow>();
            services.AddTransient<PlayerPage>();

            // Регистрация страниц
            services.AddTransient<ProjectsPage>();
            services.AddTransient<AdminPage>();
            services.AddTransient<OrganizerPage>();
            services.AddTransient<ReviewsPage>();
            services.AddTransient<StaffPage>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<DiceRollerPage>(provider =>
                            new DiceRollerPage(
                                provider.GetRequiredService<IAuthService>(),
                                provider.GetRequiredService<JwtService>()
                            ));

            services.AddTransient<IEmailTemplateService, EmailTemplateService>();
            services.AddTransient<IEmailSenderService, EmailSenderService>();
            services.AddTransient<IDatabaseService, DatabaseService>();
            // Email сервисы
            services.AddScoped<IEmailSenderService, EmailSenderService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddSingleton<ILogger<MainWindow>>(provider =>
                       provider.GetRequiredService<ILoggerFactory>().CreateLogger<MainWindow>());

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddScoped<EventsService>();

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings:Default"));

            // Настройка логирования
            services.AddLogging(configure => configure
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug));
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Дополнительная логика запуска при необходимости
        }
        public void SwitchToMainWindow()
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            Current.MainWindow?.Close();
            Current.MainWindow = mainWindow;
        }

        public void SwitchToAuthWindow()
        {
            var authWindow = ServiceProvider.GetRequiredService<Autorizatsaya>();
            authWindow.Show();
            Current.MainWindow?.Close();
            Current.MainWindow = authWindow;
        }
    }
}
