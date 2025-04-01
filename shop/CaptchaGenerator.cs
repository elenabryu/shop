using System;
using System.Windows.Media.Imaging;
using System.IO;
using System.Linq;

namespace shop
{
    public class CaptchaGenerator
    {
        private static readonly Random Random = new Random();

        public static string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static BitmapImage GenerateImage(string captchaCode)
        {
            int width = 120;
            int height = 50;

            var bitmap = new System.Drawing.Bitmap(width, height);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.Clear(System.Drawing.Color.White);

                var font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
                var color = System.Drawing.Color.Black;
                var point = new System.Drawing.PointF(10, 10);
                graphics.DrawString(captchaCode, font, new System.Drawing.SolidBrush(color), point);
            }
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); 
                return bitmapImage;
            }
        }
    }
}