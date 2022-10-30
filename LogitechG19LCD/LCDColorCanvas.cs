using System;
using System.Drawing;

namespace G19LCD
{
    public class LCDColorCanvas : IDisposable
    {
        private Graphics _graphics;

        public Graphics Graphics
        {
            get
            {
                if (_graphics == null)
                    _graphics = Graphics.FromImage(LogitechColorLCD.Bitmap);

                return _graphics;
            }
        }

        public void Update() =>
            LogitechColorLCD.Update();

        public int Width  => LogitechColorLCD.Width;
        public int Height => LogitechColorLCD.Height;

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