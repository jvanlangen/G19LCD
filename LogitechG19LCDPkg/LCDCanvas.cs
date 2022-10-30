using System;
using System.Drawing;

namespace G19LCD
{
    public class LCDCanvas : IDisposable
    {
        private Graphics _graphics;

        public Graphics Graphics
        {
            get
            {
                if (_graphics == null)
                    _graphics = Graphics.FromImage(LogitechG19LCDBitmap.Bitmap);

                return _graphics;
            }
        }

        public void Update() =>
            LogitechG19LCDBitmap.Update();

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