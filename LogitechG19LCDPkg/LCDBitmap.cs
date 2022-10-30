using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace G19LCD
{
    public static class LogitechG19LCDBitmap
    {
        private static Bitmap _bitmap;

        public static Bitmap Bitmap
        {
            get
            {
                if (_bitmap == null)
                    _bitmap = new Bitmap(320, 240, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                return _bitmap;
            }
        }

        static LogitechG19LCDBitmap()
        {
            var result = LogitechGSDK.LogiLcdInit(Assembly.GetEntryAssembly().GetName().FullName, LogitechGSDK.LOGI_LCD_TYPE_COLOR);
            if (!result)
                throw new Exception("Logitech G19 LCD is not found.");
        }

        public static int Width { get; } = 320;
        public static int Height { get; } = 240;


        public static LCDCanvas LcdBitmap() =>
            new LCDCanvas();

        public static void Update()
        {
            var data = _bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                // copy pixels..
                LogitechGSDK.LogiLcdColorSetBackground(data.Scan0);
            }
            finally
            {
                Bitmap.UnlockBits(data);
                LogitechGSDK.LogiLcdUpdate();
            }
        }

        public static void Free()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
    }
}