
namespace NRI.Services
{
    public static class EmailTemplates
    {
        public static string GetRegistrationConfirmationTemplate(string confirmationCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Подтверждение регистрации в NRI</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f9f9f9;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
            padding: 30px;
        }}
        .header {{
            text-align: center;
            margin-bottom: 25px;
        }}
        .logo {{
            max-width: 150px;
            margin-bottom: 15px;
        }}
        h1 {{
            color: #2c3e50;
            font-size: 24px;
            margin-bottom: 20px;
        }}
        .code-block {{
            background-color: #f0f4f8;
            border-left: 4px solid #3498db;
            padding: 15px;
            margin: 20px 0;
            font-size: 20px;
            font-weight: bold;
            text-align: center;
            color: #2c3e50;
        }}
        .instructions {{
            background-color: #f8f9fa;
            border-radius: 6px;
            padding: 15px;
            margin: 20px 0;
            font-size: 14px;
        }}
        .footer {{
            margin-top: 30px;
            text-align: center;
            font-size: 12px;
            color: #7f8c8d;
        }}
        .button {{
            display: inline-block;
            background-color: #3498db;
            color: white !important;
            text-decoration: none;
            padding: 12px 25px;
            border-radius: 4px;
            font-weight: bold;
            margin: 15px 0;
        }}
        .divider {{
            height: 1px;
            background-color: #e0e0e0;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <!-- Вставьте URL вашего логотипа вместо logo.png -->
            <img src=""https://img.freepik.com/premium-vector/pixel-art-dice-bit-game-icon-white-background_360488-55.jpg"" alt=""NRI Logo"" class=""logo"">
            <h1>Подтверждение регистрации</h1>
        </div>

        <p>Здравствуйте!</p>
        
        <p>Благодарим вас за регистрацию в системе NRI. Для завершения регистрации, пожалуйста, используйте следующий код подтверждения:</p>
        
        <div class=""code-block"">
            Ваш код подтверждения: <strong>{{{{confirmation_code}}}}</strong>
        </div>
        
        <p>Этот код будет действителен в течение 24 часов.</p>
        
        <div class=""instructions"">
            <p><strong>Как использовать код:</strong></p>
            <ol>
                <li>Откройте приложение NRI</li>
                <li>Перейдите в раздел ""Подтверждение регистрации""</li>
                <li>Введите указанный выше код</li>
                <li>Нажмите ""Подтвердить""</li>
            </ol>
        </div>

        <div class=""divider""></div>
        
        <p>Если вы не регистрировались в NRI, пожалуйста, проигнорируйте это письмо.</p>
        
        <div class=""footer"">
            <p>С уважением,<br>Команда NRI</p>
            <p>© 2023 NRI. Все права защищены.</p>
            <p>
                <small>
                    Это письмо отправлено автоматически. Пожалуйста, не отвечайте на него.<br>
                    По всем вопросам обращайтесь в <a href=""mailto:support@nri.example.com"">службу поддержки</a>.
                </small>
            </p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
