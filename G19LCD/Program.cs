using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace G19LCD
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = LogitechGSDK.LogiLcdInit("Limbje", LogitechGSDK.LOGI_LCD_TYPE_COLOR | LogitechGSDK.LOGI_LCD_TYPE_MONO);

            if (!result)
                throw new Exception("Unable to init G19 LCD");

            var pixels = new byte[320 * 240 * 4];

            int f = 0;

            var detectInputTask = Task.Run(() =>
            {
                var monoButtons = new[]
                {
                    LogitechGSDK.LOGI_LCD_MONO_BUTTON_0,
                    LogitechGSDK.LOGI_LCD_MONO_BUTTON_1,
                    LogitechGSDK.LOGI_LCD_MONO_BUTTON_2,
                    LogitechGSDK.LOGI_LCD_MONO_BUTTON_3
                };

                var lcdButtons = new[]
                {
                    LogitechGSDK.LOGI_LCD_COLOR_BUTTON_LEFT,
                    LogitechGSDK.LOGI_LCD_COLOR_BUTTON_UP,
                    LogitechGSDK.LOGI_LCD_COLOR_BUTTON_RIGHT,
                    LogitechGSDK.LOGI_LCD_COLOR_BUTTON_DOWN,
                    LogitechGSDK.LOGI_LCD_COLOR_BUTTON_OK,
                    LogitechGSDK.LOGI_LCD_COLOR_BUTTON_MENU,
                    LogitechGSDK.LOGI_LCD_COLOR_BUTTON_CANCEL,
                };

                while (true)
                {
                    var pressedMonoButtons = monoButtons.Where(i => LogitechGSDK.LogiLcdIsButtonPressed(i)).ToArray();
                    var pressedLCDButtons = lcdButtons.Where(i => LogitechGSDK.LogiLcdIsButtonPressed(i)).ToArray();

                    if (pressedMonoButtons.Any())
                        Console.WriteLine(string.Join(", ", pressedMonoButtons));

                    if (pressedLCDButtons.Any())
                        Console.WriteLine(string.Join(", ", pressedLCDButtons));

                    Thread.Sleep(10);
                }
            });

            while (true)
            {
                if (LogitechGSDK.LogiLcdIsConnected(LogitechGSDK.LOGI_LCD_TYPE_COLOR))
                {
                    int i = 0;
                    float ypos = 120 + (120 * MathF.Sin(f / 15f));

                    for (int y = 0; y < 240; y++)
                        for (int x = 0; x < 320; x++)
                        {
                            pixels[i++] = (byte)(127 + (63 * MathF.Sin((f + x) / 13f)));
                            pixels[i++] = (byte)(127 + (63 * MathF.Sin((f + y) / 12f)));
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
    }
}