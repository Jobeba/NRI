using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(ILogger<SmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phone, string message)
    {
        _logger.LogInformation($"Mock SMS to {phone}: {message}");
        return Task.CompletedTask;
    }
}
