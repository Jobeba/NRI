using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.Classes.Email
{
    public class SomeService
    {
        private readonly IEmailSenderService _emailSender;

        public SomeService(IEmailSenderService emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task SendEmails()
        {
            // Отправка через Mail.ru
            bool mailRuResult = await _emailSender.SendEmailAsync(
                "Default",
                "recipient@example.com",
                "Тема письма",
                "Текст письма");

            // Отправка через Yandex
            bool yandexResult = await _emailSender.SendEmailAsync(
                "Yandex",
                "recipient@example.com",
                "Тема письма",
                "Текст письма");

            // Проверка соединения
            bool isMailRuAvailable = await _emailSender.TestConnectionAsync("Default");
            bool isYandexAvailable = await _emailSender.TestConnectionAsync("Yandex");
        }
    }
}
