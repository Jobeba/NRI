using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


    public interface ICaptchaService
    {
        BitmapImage GenerateCaptcha(out string solution);
    }

    public class CaptchaService : ICaptchaService
    {
        public BitmapImage GenerateCaptcha(out string solution)
        {
            var text = GenerateRandomText();
            solution = text;

            using var bitmap = new Bitmap(200, 80);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            graphics.DrawString(text, new Font("Arial", 30), Brushes.Black, new PointF(10, 20));

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            return image;
        }

        private string GenerateRandomText()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

