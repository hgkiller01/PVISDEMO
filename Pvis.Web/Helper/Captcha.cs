using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Pvis.Web.Helper
{
    /*
     * 網路參考資料
     *  Generate Captcha Code in ASP.NET Core
     *  https://edi.wang/post/2018/10/13/generate-captcha-code-aspnet-core
     */
    public static class Captcha
    {
        
        const string Letters = "2346789ABCDEFGHJKLMNPRTUVWXYZ";
        const string SessionKey = "CaptchaCode";

        private static string GenerateCaptchaCode()
        {
            Random rand = new Random();
            int maxRand = Letters.Length - 1;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                //int index = rand.Next(maxRand);
                int index = RandomIntFromRNG(0, maxRand);
                sb.Append(Letters[index]);
            }

            return sb.ToString();
        }
        internal static Stream GetWave(HttpContext context)
        {
            string SessionCaptcha = context.Session.GetString(SessionKey);
            if (string.IsNullOrWhiteSpace(SessionCaptcha)) return null;
            WaveIO wa = new WaveIO();
            var OutWave = Path.GetRandomFileName();
            wa.Merge(
                SessionCaptcha.ToLower().Select(x => Path.Combine(Environment.CurrentDirectory, "CharWave", x + ".wav")).ToArray(),
                OutWave
            );
            if (!File.Exists(OutWave)) return null;
            var Result = new MemoryStream(File.ReadAllBytes(OutWave));
            File.Delete(OutWave);
            return Result;
        }
        private static int RandomIntFromRNG(int min = 0, int max = 0)
        {
            // Generate four random bytes
            byte[] four_bytes = new byte[4];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(four_bytes);

            // Convert the bytes to a UInt32
            UInt32 scale = BitConverter.ToUInt32(four_bytes, 0);

            // And use that to pick a random number >= min and < max
            return (int)(min + (max - min) * (scale / (uint.MaxValue + 1.0)));
        }
        internal static CaptchaResult GenerateCaptchaImage(int width, int height, HttpContext httpContext)
        {
            var captchaCode = GenerateCaptchaCode();
            var result = GenerateCaptchaImage(width, height, captchaCode);
            httpContext.Session.SetString(SessionKey, result.CaptchaCode);
            return result;
        }

        public static bool ValidateCaptchaCode(string userInputCaptcha, HttpContext context)
        {
            string SessionCaptcha = context.Session.GetString(SessionKey) ?? string.Empty;
            userInputCaptcha = userInputCaptcha ?? String.Empty;
            context.Session.Remove(SessionKey);
            if (string.IsNullOrWhiteSpace(userInputCaptcha) || string.IsNullOrWhiteSpace(SessionCaptcha)) return false;
            return userInputCaptcha.Equals(SessionCaptcha, StringComparison.OrdinalIgnoreCase);
        }

        private static CaptchaResult GenerateCaptchaImage(int width, int height, string captchaCode)
        {
#pragma warning disable CA1416 // 驗證平台相容性
            using (Bitmap baseMap = new Bitmap(width, height))

            using (Graphics graph = Graphics.FromImage(baseMap))
            {
                Random rand = new Random();

                graph.Clear(GetRandomLightColor());

                DrawCaptchaCode();
                DrawDisorderLine();
                //AdjustRippleEffect();

                MemoryStream ms = new MemoryStream();

                baseMap.Save(ms, ImageFormat.Png);

                return new CaptchaResult { CaptchaCode = captchaCode, CaptchaByteData = ms.ToArray(), Timestamp = DateTime.Now };

                int GetFontSize(int imageWidth, int captchCodeCount)
                {
                    var averageSize = imageWidth / captchCodeCount;

                    return Convert.ToInt32(averageSize);
                }

                Color GetRandomDeepColor()
                {
                    int redlow = 160, greenLow = 100, blueLow = 160;
                    return Color.FromArgb(RandomIntFromRNG(0,redlow), RandomIntFromRNG(0,greenLow), RandomIntFromRNG(0,blueLow));
                }

                Color GetRandomLightColor()
                {
                    int low = 180, high = 255;

                    int nRend = RandomIntFromRNG(0,high) % (high - low) + low;
                    int nGreen = RandomIntFromRNG(0,high) % (high - low) + low;
                    int nBlue = RandomIntFromRNG(0,high) % (high - low) + low;

                    return Color.FromArgb(nRend, nGreen, nBlue);
                }

                void DrawCaptchaCode()
                {
                    SolidBrush fontBrush = new SolidBrush(Color.Black);
                    int fontSize = GetFontSize(width, captchaCode.Length);
                    Font font = new Font(FontFamily.GenericSerif, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                    for (int i = 0; i < captchaCode.Length; i++)
                    {
                        fontBrush.Color = GetRandomDeepColor();

                        int shiftPx = fontSize / 6;

                        float x = i * fontSize + RandomIntFromRNG(-shiftPx, shiftPx) + RandomIntFromRNG(-shiftPx, shiftPx);
                        int maxY = height - fontSize;
                        if (maxY < 0) maxY = 0;
                        float y = RandomIntFromRNG(0, maxY);

                        graph.DrawString(captchaCode[i].ToString(), font, fontBrush, x, y);
                    }
                }

                void DrawDisorderLine()
                {
                    Pen linePen = new Pen(new SolidBrush(Color.Black), 1);
                    for (int i = 0; i < RandomIntFromRNG(3, 5); i++)
                    {
                        linePen.Color = GetRandomDeepColor();

                        Point startPoint = new Point(RandomIntFromRNG(0, width), RandomIntFromRNG(0, height));
                        Point endPoint = new Point(RandomIntFromRNG(0, width), RandomIntFromRNG(0, height));
                        graph.DrawLine(linePen, startPoint, endPoint);

                        Point bezierPoint1 = new Point(RandomIntFromRNG(0, width), RandomIntFromRNG(0, height));
                        Point bezierPoint2 = new Point(RandomIntFromRNG(0, width), RandomIntFromRNG(0, height));

                        graph.DrawBezier(linePen, startPoint, bezierPoint1, bezierPoint2, endPoint);
                    }
                }

            }
#pragma warning restore CA1416 // 驗證平台相容性
        }
    }
}
