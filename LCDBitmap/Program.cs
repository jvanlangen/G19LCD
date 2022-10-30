using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace G19LCD
{

    class Program
    {
        static void Main(string[] args)
        {
            var brushes = new[] { Brushes.Red, Brushes.Yellow, Brushes.Blue, Brushes.Green, Brushes.Purple, Brushes.Orange, Brushes.Lime };
            var colors = new[] { Color.Red, Color.Yellow, Color.Blue, Color.Green, Color.Purple, Color.Orange, Color.Lime };
            var pens = colors.Select(c => new Pen(c)).ToArray();

            var rnd = new Random();

            LogitechColorLCD.ButtonMenuClicked += LogitechColorLCD_ButtonMenuClicked;

            using (var canvas = LogitechColorLCD.GetCanvas())
            {
                for (int i = 0; i < 1000; i++)
                {
                    var x = rnd.Next(canvas.Width);
                    var y = rnd.Next(canvas.Height);
                    var w = rnd.Next(canvas.Width - x);
                    var h = rnd.Next(canvas.Height - y);
                    canvas.Graphics.DrawRectangle(pens[rnd.Next(pens.Length)], new Rectangle(x, y, w, h));
                }
            }

            LogitechMonoLCD.Button0Clicked += LogitechMonoLCD_Button0Clicked;

            using (var canvas = LogitechMonoLCD.GetCanvas())
            {
                for (int i = 0; i < 100; i++)
                {
                    var x = rnd.Next(canvas.Width);
                    var y = rnd.Next(canvas.Height);
                    var w = rnd.Next(canvas.Width - x);
                    var h = rnd.Next(canvas.Height - y);
                    canvas.Graphics.DrawRectangle(Pens.White, new Rectangle(x, y, w, h));
                }
            }

            DrawColors();

//            Console.ReadLine();
        }

        private static void DrawColors()
        {
            var pixels = new byte[320 * 240 * 4];

            int f = 0;

            // draw colors loop
            while (true)
            {
                if (LogitechGSDK.LogiLcdIsConnected(LogitechGSDK.LOGI_LCD_TYPE_COLOR))
                {
                    int i = 0;
                    double ypos = 120 + (120 * Math.Sin(f / 15f));

                    for (int y = 0; y < 240; y++)
                        for (int x = 0; x < 320; x++)
                        {
                            pixels[i++] = (byte)(127 + (63 * Math.Sin((f + x) / 13f)));
                            pixels[i++] = (byte)(127 + (63 * Math.Sin((f + y) / 12f)));
                            pixels[i++] = (byte)(255 - Math.Abs(y - ypos));
                            pixels[i++] = 255;
                        }
                    f++;

                    LogitechGSDK.LogiLcdColorSetBackground(pixels);
                }

                if (LogitechGSDK.LogiLcdIsConnected(LogitechGSDK.LOGI_LCD_TYPE_MONO))
                {
                    LogitechGSDK.LogiLcdMonoSetText(0, "bla bla");
                }

                LogitechGSDK.LogiLcdUpdate();
                Thread.Sleep(10);
            }
        }

        private static void LogitechMonoLCD_Button0Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Button 0 clicked");
        }

        private static void LogitechColorLCD_ButtonMenuClicked(object sender, EventArgs e)
        {
            Console.WriteLine("Menu button clicked");
        }
    }
}