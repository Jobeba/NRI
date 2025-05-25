using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace NRI
{
    // Вспомогательный класс для работы с CAPTCHA
    public static class CaptchaHelper
    {
        // Генерация случайной строки для CAPTCHA
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Генерация изображения CAPTCHA
        public static BitmapImage GenerateCaptchaImage(string text,
            int width = 200,
            int height = 80,
            string fontFamily = "Arial",
            double fontSize = 28,
            Brush textColor = null,
            Brush backgroundColor = null,
            int noiseLines = 10,
            int distortionLevel = 5)
        {
            textColor ??= Brushes.Navy;
            backgroundColor ??= Brushes.White;

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();

            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(backgroundColor, null, new Rect(0, 0, width, height));

                var random = new Random();
                for (int i = 0; i < text.Length; i++)
                {
                    var formattedText = new FormattedText(
                        text[i].ToString(),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(fontFamily),
                        fontSize + random.Next(-distortionLevel, distortionLevel + 1),
                        textColor,
                        VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                    context.PushTransform(new TranslateTransform(
                        i * 35 + random.Next(-distortionLevel, distortionLevel + 1),
                        30 + random.Next(-distortionLevel * 2, distortionLevel * 2 + 1)));
                    context.PushTransform(new RotateTransform(random.Next(-15, 15)));
                    context.DrawText(formattedText, new Point(0, 0));
                    context.Pop();
                    context.Pop();
                }

                for (int i = 0; i < noiseLines; i++)
                {
                    context.DrawLine(
                        new Pen(Brushes.Gray, 1),
                        new Point(random.Next(0, width), random.Next(0, height)),
                        new Point(random.Next(0, width), random.Next(0, height)));
                }

                bitmap.Render(visual);

                var bitmapImage = new BitmapImage();
                using (var stream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);

                    stream.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }

                return bitmapImage;
            }
        }
    }
}
