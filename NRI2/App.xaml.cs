using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.API;
using NRI.Classes;
using NRI.Classes.Email;
using NRI.Controls;
using NRI.Data;
using NRI.DiceRoll;
using NRI.Models;
using NRI.Pages;
using NRI.Services;
using NRI.ViewModels;
using NRI.Windows;
using System;
using System.IdentityModel.Claims;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NRI
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider;
        public static IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                Configuration = builder.Build();

                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                ServiceProvider = serviceCollection.BuildServiceProvider();

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

            services.AddHttpClient("ActivityClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<UserActivityService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiBaseUrl"]);
            });

            // Регистрация сервисов
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
            services.AddSingleton<ILogger<Autorizatsaya>>(provider =>
                        provider.GetRequiredService<ILoggerFactory>().CreateLogger<Autorizatsaya>());
            services.AddSingleton<ILogger<MainWindow>>(provider =>
                    provider.GetRequiredService<ILoggerFactory>().CreateLogger<MainWindow>());
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<Func<int>>(provider =>
            {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                return () =>
                {
                    var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return int.TryParse(userId, out var id) ? id : 0;
                };
            });

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
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IGameSystemService, GameSystemService>();
            services.AddTransient<JwtService>();
            services.AddTransient<IUserActivityService, UserActivityService>();

            services.AddScoped<ApiClient>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("ApiClient");
                var jwtService = provider.GetRequiredService<JwtService>();
                return new ApiClient(httpClient, jwtService);
            });

            // Регистрация контекста БД

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptions => sqlServerOptions.MigrationsAssembly("NRI")));

            // Регистрация ViewModels
            services.AddSingleton<MainWindowViewModel>(provider =>
            {
                var apiClient = provider.GetRequiredService<ApiClient>();
                var logger = provider.GetRequiredService<ILogger<MainWindowViewModel>>();
                return new MainWindowViewModel(apiClient, logger);
            });


            services.AddTransient<DiceRollerViewModel>();
            services.AddTransient<AdminViewModel>();
            // Регистрация Controls
            services.AddTransient<BaseWindowControl>();
            services.AddTransient<AdminWindowControl>();
            services.AddTransient<OrganizerWindowControl>();
            services.AddTransient<PlayerWindowControl>();

            // Регистрация окон
            services.AddTransient<Autorizatsaya>();

            services.AddSingleton<MainWindow>(provider =>
            {
                var viewModel = provider.GetRequiredService<MainWindowViewModel>();
                var window = new MainWindow(
                    provider,
                    provider.GetRequiredService<ILogger<MainWindow>>(),
                    provider.GetRequiredService<INavigationService>(),
                    provider.GetRequiredService<JwtService>(),
                    provider.GetRequiredService<UserActivityService>())
                {
                    DataContext = viewModel
                };
                viewModel.Initialize(provider); // Инициализация сервис-провайдера
                return window;
            });

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
            services.AddTransient<INavigationService, NavigationService>();

            // Email сервисы
            services.AddScoped<IEmailSenderService, EmailSenderService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserPresenceService, UserPresenceService>();
            services.AddSingleton<ILogger<MainWindow>>(provider =>
                       provider.GetRequiredService<ILoggerFactory>().CreateLogger<MainWindow>());
            services.AddScoped<UserActivityService>();
            services.AddScoped<IGameSystemService, GameSystemService>();
            services.AddHttpContextAccessor();

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
