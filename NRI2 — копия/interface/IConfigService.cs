using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NRI.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IConfigService
{
    string GetConnectionString();
    string SmtpHost { get; }
    int SmtpPort { get; }
    string SmtpUsername { get; }
    string SmtpPassword { get; }
    EmailSettings GetEmailSettings(string provider);

}

    public class ConfigService : IConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigService> _logger;

    public ConfigService(IConfiguration configuration, ILogger<ConfigService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Debug.WriteLine("ConfigService initialized with connection string: " +
            _configuration.GetConnectionString("DefaultConnection"));

        ValidateConnectionString();
    }

    public string GetConnectionString()
    {
        var connString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connString))
        {
            _logger.LogError("Connection string 'DefaultConnection' not found in configuration");
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration");
        }
        return connString;
    }

    private void ValidateConnectionString()
    {
        try
        {
            var connString = GetConnectionString();
            _logger.LogInformation($"Using connection string: {connString}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating connection string");
            throw;
        }
    }

    public string SmtpHost => _configuration["EmailSettings:SmtpHost"];

        public int SmtpPort
        {
            get
            {
                if (int.TryParse(_configuration["EmailSettings:SmtpPort"], out int port))
                    return port;
                throw new InvalidOperationException("Некорректный порт SMTP в настройках");
            }
        }

        public string SmtpUsername => _configuration["EmailSettings:SmtpUsername"];

        public string SmtpPassword => _configuration["EmailSettings:SmtpPassword"];

    public EmailSettings GetEmailSettings(string provider)
    {
        var settings = _configuration.GetSection($"EmailSettings:{provider}").Get<EmailSettings>();

        if (settings == null)
        {
            throw new InvalidOperationException($"Настройки для провайдера {provider} не найдены");
        }

        // Дополнительная валидация
        if (settings.SmtpPort <= 0 || string.IsNullOrEmpty(settings.SmtpHost))
        {
            throw new InvalidOperationException($"Некорректные настройки SMTP для провайдера {provider}");
        }

        return settings;
    }
}

