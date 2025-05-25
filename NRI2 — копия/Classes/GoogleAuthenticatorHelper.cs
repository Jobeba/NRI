using OtpNet;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

public static class GoogleAuthenticatorHelper
{
    public static string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public static BitmapImage GenerateQrCode(string accountName, string secretKey, string issuer = "NRI System")
    {
        string qrCodeContent = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}&digits=6";

        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeContent, QRCodeGenerator.ECCLevel.Q);
        QRCode qrCode = new QRCode(qrCodeData);

        using (var bitmap = qrCode.GetGraphic(20))
        using (var memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memory;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }

    public static bool ValidateCode(string secretKey, string code, int period = 30)
    {
        try
        {
            var totp = new Totp(Base32Encoding.ToBytes(secretKey), period);
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            return false;
        }
    }
}
