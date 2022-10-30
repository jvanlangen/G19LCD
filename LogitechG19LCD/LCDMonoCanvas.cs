using System;
using System.Drawing;

namespace G19LCD
{
    public class LCDMonoCanvas : IDisposable
    {
        private Graphics _graphics;

        public Graphics Graphics
        {
            get
            {
                if (_graphics == null)
                    _graphics = Graphics.FromImage(LogitechMonoLCD.Bitmap);

                return _graphics;
            }
        }

        public void Update() =>
            LogitechMonoLCD.Update();

        public int Width => LogitechMonoLCD.Width;
        public int Height => LogitechMonoLCD.Height;

        public void Dispose()
        {
            Update();

            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }
        }
    }
}