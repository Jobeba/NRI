using System;

namespace NRI.Classes.Email
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GetRegistrationTemplate(string confirmationCode, string fullName = null)
        {
            string greeting = GetPixelGreeting(fullName);
            return BuildPixelEmailTemplate(
                "РЕГИСТРАЦИЯ",
                $@"
                <div class='pixel-greeting'>{greeting}</div>
                
                <div class='pixel-text'>ВАШ КОД ПОДТВЕРЖДЕНИЯ:</div>
                
                <div class='pixel-code'>
                    {confirmationCode}
                </div>
                
                <div class='pixel-small'>※ ДЕЙСТВИТЕЛЕН 24 ЧАСА ※</div>
                
                <div class='pixel-instructions'>
                    <div class='pixel-step'>▶ ЗАПУСТИТЕ ИГРУ NRI</div>
                    <div class='pixel-step'>▶ ВВЕДИТЕ ЭТОТ КОД</div>
                    <div class='pixel-step'>▶ НАЖМИТЕ ""ПОДТВЕРДИТЬ""</div>
                </div>"
            );
        }

        public string GetPasswordChangeConfirmationTemplate(string confirmationCode, string fullName = null)
        {
            string greeting = GetPixelGreeting(fullName);
            return BuildPixelEmailTemplate(
                "СМЕНА ПАРОЛЯ",
                $@"
                <div class='pixel-greeting'>{greeting}</div>
                
                <div class='pixel-text'>КОД ПОДТВЕРЖДЕНИЯ СМЕНЫ ПАРОЛЯ:</div>
                
                <div class='pixel-code'>
                    {confirmationCode}
                </div>
                
                <div class='pixel-small'>※ ДЕЙСТВИТЕЛЕН 15 МИНУТ ※</div>
                
                <div class='pixel-warning'>
                    ⚠ ЕСЛИ ВЫ НЕ ЗАПРАШИВАЛИ СМЕНУ ПАРОЛЯ, ⚠<br>
                    ⚠ НЕМЕДЛЕННО СМЕНИТЕ ПАРОЛЬ ОТ СВОЕГО АККАУНТА ⚠
                </div>
                
                <div class='pixel-instructions'>
                    <div class='pixel-step'>▶ ВВЕДИТЕ ЭТОТ КОД В ФОРМЕ ПОДТВЕРЖДЕНИЯ</div>
                    <div class='pixel-step'>▶ ВВЕДИТЕ НОВЫЙ ПАРОЛЬ</div>
                    <div class='pixel-step'>▶ СОХРАНИТЕ ЕГО В БЕЗОПАСНОМ МЕСТЕ</div>
                </div>"
            );
        }

        public string GetTwoFactorSetupTemplate(string secretKey, string fullName = null)
        {
            string greeting = GetPixelGreeting(fullName);
            return BuildPixelEmailTemplate(
                "2FA АКТИВАЦИЯ",
                $@"
                <div class='pixel-greeting'>{greeting}</div>
                
                <div class='pixel-text'>ВАШ СЕКРЕТНЫЙ КЛЮЧ:</div>
                
                <div class='pixel-code'>
                    {secretKey}
                </div>
                
                <div class='pixel-small'>※ СОХРАНИТЕ В БЕЗОПАСНОМ МЕСТЕ ※</div>
                
                <div class='pixel-instructions'>
                    <div class='pixel-step'>▶ УСТАНОВИТЕ AUTH APP</div>
                    <div class='pixel-step'>▶ ВВЕДИТЕ ЭТОТ КЛЮЧ</div>
                    <div class='pixel-step'>▶ СОХРАНИТЕ РЕЗЕРВНЫЕ КОДЫ</div>
                </div>"
            );
        }

        public string GetPasswordResetTemplate(string resetLink)
        {
            return BuildPixelEmailTemplate(
                "СБРОС ПАРОЛЯ",
                $@"
                <div class='pixel-text'>НАЖМИТЕ ДЛЯ СБРОСА ПАРОЛЯ:</div>
                
                <a href='{resetLink}' class='pixel-button'>
                    [ СБРОСИТЬ ПАРОЛЬ ]
                </a>
                
                <div class='pixel-warning'>
                    ⚠ ССЫЛКА ДЕЙСТВИТЕЛЬНА 24 ЧАСА ⚠
                </div>"
            );
        }

        private string BuildPixelEmailTemplate(string title, string content)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title} | NRI</title>
    <style>
        body {{
            background-color: #372850;
            background-image: 
                url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IArs4c6QAAAERlWElmTU0AKgAAAAgAAYdpAAQAAAABAAAAGgAAAAAAA6ABAAMAAAABAAEAAKACAAQAAAABAAAAFKADAAQAAAABAAAAFAAAAACy3fD9AAAASElEQVQ4EWP8//8/AzUBEwOVwaiBQ9hC5v/+e/v/v//+///39x/wgQHi/v8H0wBxEB7yAIT/9/8fEIPEQTgMjxYH4v/BD0IWCuOh5kIvDjQKRsEoGAWjYHgAAAuRGpLtlshRAAAAAElFTkSuQmCC');
            color: #f8f7a0;
            font-family: 'Courier New', monospace;
            font-size: 14px;
            line-height: 1.4;
            margin: 0;
            padding: 16px;
            image-rendering: pixelated;
        }}
        
        .pixel-container {{
            max-width: 320px;
            margin: 0 auto;
            background-color: #2a2742;
            border: 4px solid #eb6f92;
            box-shadow: 8px 8px 0 #1a1530;
            position: relative;
            padding: 2px;
        }}
        
        .pixel-header {{
            background-color: #5a6ee1;
            color: #000;
            padding: 12px;
            text-align: center;
            border-bottom: 4px solid #eb6f92;
            font-weight: bold;
            letter-spacing: 1px;
            text-transform: uppercase;
            margin: -2px -2px 12px -2px;
        }}
        
        .pixel-body {{
            padding: 16px;
        }}
        
        .pixel-greeting {{
            color: #74d1e5;
            margin-bottom: 16px;
            font-size: 16px;
            text-shadow: 2px 2px #1a1530;
            text-align: center;
        }}
        
        .pixel-text {{
            color: #f8f7a0;
            text-shadow: 2px 2px #1a1530;
            margin: 12px 0;
        }}
        
        .pixel-code {{
            background-color: #1a1530;
            border: 2px dashed #9ccfd8;
            padding: 12px;
            margin: 16px 0;
            text-align: center;
            font-family: 'Courier New', monospace;
            font-size: 20px;
            font-weight: bold;
            color: #9ccfd8;
            letter-spacing: 2px;
        }}
        
        .pixel-small {{
            font-size: 10px;
            color: #ebbcba;
            text-align: center;
            margin: 8px 0;
        }}
        
        .pixel-button {{
            display: block;
            background-color: #eb6f92;
            color: #000;
            padding: 12px;
            margin: 20px auto;
            text-align: center;
            text-decoration: none;
            font-weight: bold;
            border: none;
            box-shadow: 4px 4px 0 #1a1530;
            width: 90%;
        }}
        
        .pixel-instructions {{
            margin: 20px 0;
            border-top: 2px dotted #ebbcba;
            padding-top: 16px;
        }}
        
        .pixel-step {{
            margin-bottom: 12px;
            color: #f8f7a0;
        }}
        
        .pixel-warning {{
            color: #eb6f92;
            text-align: center;
            font-size: 12px;
            margin: 20px 0;
            text-shadow: 1px 1px #1a1530;
        }}
        
        .pixel-footer {{
            background-color: #1a1530;
            padding: 8px;
            text-align: center;
            border-top: 4px solid #5a6ee1;
            font-size: 10px;
            color: #6e6a86;
            margin: -2px -2px -2px -2px;
        }}
    </style>
</head>
<body>
    <div class='pixel-container'>
        <div class='pixel-header'>※ {title.ToUpper()} ※</div>
        
        <div class='pixel-body'>
            {content}
        </div>
        
        <div class='pixel-footer'>
            © {DateTime.Now.Year} NRI RPG | 8-BIT EDITION
        </div>
    </div>
</body>
</html>";
        }

        private string GetPixelGreeting(string fullName)
        {
            var hour = DateTime.Now.Hour;
            string timeGreeting = hour switch
            {
                >= 5 and < 12 => "ДОБРОЕ УТРО",
                >= 12 and < 18 => "ДОБРЫЙ ДЕНЬ",
                _ => "ДОБРЫЙ ВЕЧЕР"
            };

            return string.IsNullOrWhiteSpace(fullName)
                ? $"{timeGreeting}, ИГРОК!"
                : $"{timeGreeting}, {fullName.ToUpper()}!";
        }
    }
}
