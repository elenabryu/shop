using System;
using System.Windows.Media.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

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
            int width = 150;
            int height = 60;
            int fontSize = 25;

            var bitmap = new System.Drawing.Bitmap(width, height);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.Clear(System.Drawing.Color.White);

                var font = new System.Drawing.Font("Arial", fontSize, System.Drawing.FontStyle.Bold);
                var color = System.Drawing.Color.Black;

                for (int i = 0; i < captchaCode.Length; i++)
                {
                    float x = 10 + (width - 20) * i / captchaCode.Length;
                    float y = 10 + Random.Next(-5, 5);
                    float angle = Random.Next(-20, 20);

                    using (var matrix = new System.Drawing.Drawing2D.Matrix())
                    {
                        matrix.RotateAt(angle, new System.Drawing.PointF(x + fontSize / 2, y + fontSize / 2));
                        graphics.Transform = matrix;
                        graphics.DrawString(captchaCode[i].ToString(), font, new System.Drawing.SolidBrush(color), x, y);
                        graphics.ResetTransform();
                    }

                    int lineX1 = Random.Next(0, width);
                    int lineY1 = Random.Next(0, height);
                    int lineX2 = Random.Next(0, width);
                    int lineY2 = Random.Next(0, height);
                    graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Gray, 2), lineX1, lineY1, lineX2, lineY2);

                    if (i < captchaCode.Length - 1 && Random.Next(0, 2) == 0) 
                    {
                        float overlapX = 10 + (width - 20) * (i + 0.5f) / captchaCode.Length; 
                        float overlapY = 10 + Random.Next(-5, 5);
                        float overlapAngle = Random.Next(-20, 20);

                        using (var matrix = new System.Drawing.Drawing2D.Matrix())
                        {
                            matrix.RotateAt(overlapAngle, new System.Drawing.PointF(overlapX + fontSize / 2, overlapY + fontSize / 2));
                            graphics.Transform = matrix;
                            graphics.DrawString(captchaCode[i + 1].ToString(), font, new System.Drawing.SolidBrush(System.Drawing.Color.LightGray), overlapX, overlapY);
                            graphics.ResetTransform();
                        }
                    }

                }


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