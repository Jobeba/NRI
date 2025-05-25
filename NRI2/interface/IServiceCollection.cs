using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет сервисы приложения в контейнер DI
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Логирование
            services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.AddDebug();
            });

            // Сервисы отправки сообщений
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmsSender, SmsSender>();

            // Репозитории и сервисы
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IConfigService, ConfigService>();

            return services;
        }
    }
}
