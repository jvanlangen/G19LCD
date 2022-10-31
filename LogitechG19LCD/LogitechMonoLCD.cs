using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace G19LCD
{
    public static unsafe class LogitechMonoLCD
    {
        private static Bitmap _bitmap;

        public static readonly bool Initialized;

        private static readonly SynchronizationContext _context;
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public static Bitmap Bitmap
        {
            get
            {
                if (_bitmap == null)
                    _bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                return _bitmap;
            }
        }

        static LogitechMonoLCD()
        {
            if (!LogitechLCD.Initialized || !LogitechLCD.MonoLCDConnected)
                return;

            _context = SynchronizationContext.Current;

            if (_context == null)
            {
                Trace.TraceWarning("Current thread doesn't have a SynchronizationContext");
                _context = new SynchronizationContext(); // threadpool
            }

            var monoLCDButtons = new LcdToggleButton[]
            {
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_MONO_BUTTON_0,
                    Down = (s, e) => Button0Pressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        Button0Clicked?.Invoke(s, e);
                        Button0Released?.Invoke(s, e);
                    }
                },
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_MONO_BUTTON_1,
                    Down = (s, e) => Button1Pressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        Button1Clicked?.Invoke(s, e);
                        Button1Released?.Invoke(s, e);
                    }
                },
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_MONO_BUTTON_2,
                    Down = (s, e) => Button2Pressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        Button2Clicked?.Invoke(s, e);
                        Button2Released?.Invoke(s, e);
                    }
                },
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_MONO_BUTTON_3,
                    Down = (s, e) => Button3Pressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        Button3Clicked?.Invoke(s, e);
                        Button3Released?.Invoke(s, e);
                    }
                },
            };

            EventHandler[] handlers = new EventHandler[monoLCDButtons.Length];

            Task.Run(() =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    int count = 0;

                    // check the button presses.
                    for (int i = 0; i < monoLCDButtons.Length; i++)
                    {
                        var handler = monoLCDButtons[i].CheckButtonEvent();
                        if (handler != null)
                            handlers[count++] = handler;
                    }

                    // if there are any handlers, post them on the synchronization context
                    if (count > 0)
                        _context.Post(s =>
                        {
                            foreach (var handler in handlers.Take(count))
                                handler(s, EventArgs.Empty);
                        }, null);

                    Thread.Sleep(10);
                }
            });
        }

        public static event EventHandler Button0Clicked;
        public static event EventHandler Button0Released;
        public static event EventHandler Button0Pressed;

        public static event EventHandler Button1Clicked;
        public static event EventHandler Button1Released;
        public static event EventHandler Button1Pressed;

        public static event EventHandler Button2Clicked;
        public static event EventHandler Button2Released;
        public static event EventHandler Button2Pressed;

        public static event EventHandler Button3Clicked;
        public static event EventHandler Button3Released;
        public static event EventHandler Button3Pressed;

        public static int Width { get; } = 160;
        public static int Height { get; } = 43;

        public static LCDMonoCanvas GetCanvas() =>
            new LCDMonoCanvas();

        private static byte[] MonoBitmap = new byte[160 * 43];

        public static unsafe void Update()
        {
            var data = _bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                unsafe
                {
                    uint* startOffset = (uint*)data.Scan0;

                    // gray=0.299red+0.587green+0.114blue

                    for (int i = 0; i < MonoBitmap.Length; i++)
                    {
                        uint value = *(startOffset++);

                        var red = (value >> 24) & 0xff;
                        var green = (value >> 16) & 0xff;
                        var blue = (value >> 8) & 0xff;

                        MonoBitmap[i] = (byte)(0.299 * red + 0.587 * green + 0.114 * blue); ;
                    }
                }

                // copy pixels..
                LogitechGSDK.LogiLcdMonoSetBackground(MonoBitmap);
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