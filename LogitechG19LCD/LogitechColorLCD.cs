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

    public static class LogitechLCD
    {
        static LogitechLCD()
        {
            Initialized = LogitechGSDK.LogiLcdInit(Assembly.GetEntryAssembly().GetName().FullName, LogitechGSDK.LOGI_LCD_TYPE_MONO | LogitechGSDK.LOGI_LCD_TYPE_COLOR);

            ColorLCDConnected = LogitechGSDK.LogiLcdIsConnected(LogitechGSDK.LOGI_LCD_TYPE_COLOR);
            MonoLCDConnected = LogitechGSDK.LogiLcdIsConnected(LogitechGSDK.LOGI_LCD_TYPE_MONO);
        }

        public static bool MonoLCDConnected { get; }
        public static bool ColorLCDConnected { get; }
        public static bool Initialized { get; }
    }

    public static class LogitechColorLCD
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
                //                _bitmap = new Bitmap(320, 240, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                return _bitmap;
            }
        }


        private struct LcdToggleButton
        {
            private bool _previousState;

            public EventHandler CheckButtonEvent()
            {
                var state = LogitechGSDK.LogiLcdIsButtonPressed(Key);

                try
                {
                    if (_previousState != state)
                    {
                        if (state)
                            return Down;
                        else
                            return Up;
                    }
                    return null;
                }
                finally
                {
                    _previousState = state;
                }
            }

            public int Key;
            public EventHandler Down;
            public EventHandler Up;
        }

        static LogitechColorLCD()
        {
            if (!LogitechLCD.Initialized || !LogitechLCD.ColorLCDConnected)
                return;

            // capture context
            _context = SynchronizationContext.Current;

            if (_context == null)
            {
                Trace.TraceWarning("Current thread doesn't have a SynchronizationContext");
                _context = new SynchronizationContext(); // threadpool
            }

            var colorLCDButtons = new LcdToggleButton[]
            {
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_COLOR_BUTTON_LEFT,
                    Down = (s, e) => ButtonLeftPressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        ButtonLeftClicked?.Invoke(s, e);
                        ButtonLeftReleased?.Invoke(s, e);
                    }
                },
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_COLOR_BUTTON_UP,
                    Down = (s, e) => ButtonUpPressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        ButtonUpClicked?.Invoke(s, e);
                        ButtonUpReleased?.Invoke(s, e);
                    }
                },
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_COLOR_BUTTON_RIGHT,
                    Down = (s, e) => ButtonRightPressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        ButtonRightClicked?.Invoke(s, e);
                        ButtonRightReleased?.Invoke(s, e);
                    }
                },
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_COLOR_BUTTON_DOWN,
                    Down = (s, e) => ButtonDownPressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        ButtonDownClicked?.Invoke(s, e);
                        ButtonDownReleased?.Invoke(s, e);
                    }
                },

                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_COLOR_BUTTON_OK,
                    Down = (s, e) => ButtonOkPressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        ButtonOkClicked?.Invoke(s, e);
                        ButtonOkReleased?.Invoke(s, e);
                    }
                },
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_COLOR_BUTTON_MENU,
                    Down = (s, e) => ButtonMenuPressed?.Invoke(s, e),
                    Up = (s, e) =>
                    {
                        ButtonMenuClicked?.Invoke(s, e);
                        ButtonMenuReleased?.Invoke(s, e);
                    }
                },
                new LcdToggleButton
                {
                    Key = LogitechGSDK.LOGI_LCD_COLOR_BUTTON_CANCEL,
                    Down = (s, e) => ButtonCancelPressed?.Invoke(s, e),
                    Up = (s, e) => 
                    { 
                        ButtonCancelClicked?.Invoke(s, e);
                        ButtonCancelReleased?.Invoke(s, e);
                    }
                },
            };

            EventHandler[] handlers = new EventHandler[colorLCDButtons.Length];

            Task.Run(() =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    int count = 0;

                    // check the button presses.
                    for (int i = 0; i < colorLCDButtons.Length; i++)
                    {
                        var handler = colorLCDButtons[i].CheckButtonEvent();
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

        public static event EventHandler ButtonLeftPressed;
        public static event EventHandler ButtonLeftReleased;
        public static event EventHandler ButtonLeftClicked;

        public static event EventHandler ButtonUpPressed;
        public static event EventHandler ButtonUpReleased;
        public static event EventHandler ButtonUpClicked;

        public static event EventHandler ButtonRightPressed;
        public static event EventHandler ButtonRightReleased;
        public static event EventHandler ButtonRightClicked;

        public static event EventHandler ButtonDownPressed;
        public static event EventHandler ButtonDownReleased;
        public static event EventHandler ButtonDownClicked;

        public static event EventHandler ButtonOkPressed;
        public static event EventHandler ButtonOkReleased;
        public static event EventHandler ButtonOkClicked;

        public static event EventHandler ButtonMenuPressed;
        public static event EventHandler ButtonMenuReleased;
        public static event EventHandler ButtonMenuClicked;

        public static event EventHandler ButtonCancelPressed;
        public static event EventHandler ButtonCancelReleased;
        public static event EventHandler ButtonCancelClicked;

        public static int Width { get; } = 320;
        public static int Height { get; } = 240;

        public static LCDColorCanvas GetCanvas() =>
            new LCDColorCanvas();

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


    //public static class LogitechColorLCD
    //{
    //    private static Bitmap _bitmap;

    //    public static readonly bool Initialized;

    //    private static readonly SynchronizationContext _context;
    //    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    //    public static Bitmap Bitmap
    //    {
    //        get
    //        {
    //            if (_bitmap == null)
    //                _bitmap = new Bitmap(320, 240, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

    //            return _bitmap;
    //        }
    //    }

    //    private static readonly int[] _colorLCDButtons = new[]
    //    {
    //        LogitechGSDK.LOGI_LCD_COLOR_BUTTON_LEFT,
    //        LogitechGSDK.LOGI_LCD_COLOR_BUTTON_UP,
    //        LogitechGSDK.LOGI_LCD_COLOR_BUTTON_RIGHT,
    //        LogitechGSDK.LOGI_LCD_COLOR_BUTTON_DOWN,
    //        LogitechGSDK.LOGI_LCD_COLOR_BUTTON_OK,
    //        LogitechGSDK.LOGI_LCD_COLOR_BUTTON_MENU,
    //        LogitechGSDK.LOGI_LCD_COLOR_BUTTON_CANCEL,
    //    };

    //    static LogitechColorLCD()
    //    {
    //        Initialized = LogitechGSDK.LogiLcdInit(Assembly.GetEntryAssembly().GetName().FullName, LogitechGSDK.LOGI_LCD_TYPE_COLOR);

    //        _context = SynchronizationContext.Current;

    //        Task.Run(() =>
    //        {
    //            while (_cancellationTokenSource.IsCancellationRequested)
    //            {
    //                // check the button presses.
    //                for (int i = 0; i < _colorLCDButtons.Length; i++)

    //                    if (LogitechGSDK.LogiLcdIsButtonPressed(_colorLCDButtons[i]))
    //                        switch (_colorLCDButtons[i])
    //                        {
    //                            case (LogitechGSDK.LOGI_LCD_COLOR_BUTTON_LEFT):
    //                                ButtonLeftPressed?.Invoke(null, EventArgs.Empty);
    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_COLOR_BUTTON_UP):
    //                                ButtonUpPressed?.Invoke(null, EventArgs.Empty);
    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_COLOR_BUTTON_RIGHT):
    //                                ButtonRightPressed?.Invoke(null, EventArgs.Empty);
    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_COLOR_BUTTON_DOWN):
    //                                ButtonDownPressed?.Invoke(null, EventArgs.Empty);
    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_COLOR_BUTTON_OK):
    //                                ButtonOKPressed?.Invoke(null, EventArgs.Empty);
    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_COLOR_BUTTON_MENU):
    //                                ButtonMenuPressed?.Invoke(null, EventArgs.Empty);
    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_COLOR_BUTTON_CANCEL):
    //                                ButtonCancelPressed?.Invoke(null, EventArgs.Empty);
    //                                break;
    //                        }


    //                Thread.Sleep(10);
    //            }
    //        });

    //    }

    //    public static event EventHandler ButtonMenuPressed;
    //    public static event EventHandler ButtonCancelPressed;
    //    public static event EventHandler ButtonOKPressed;
    //    public static event EventHandler ButtonLeftPressed;
    //    public static event EventHandler ButtonRightPressed;
    //    public static event EventHandler ButtonUpPressed;
    //    public static event EventHandler ButtonDownPressed;

    //    public static int Width { get; } = 320;
    //    public static int Height { get; } = 240;

    //    public static LCDMonoCanvas LcdBitmap() =>
    //        new LCDMonoCanvas();

    //    public static void Update()
    //    {
    //        var data = _bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    //        try
    //        {
    //            // copy pixels..
    //            LogitechGSDK.LogiLcdColorSetBackground(data.Scan0);
    //        }
    //        finally
    //        {
    //            Bitmap.UnlockBits(data);
    //            LogitechGSDK.LogiLcdUpdate();
    //        }
    //    }

    //    public static void Free()
    //    {
    //        if (_bitmap != null)
    //        {
    //            _bitmap.Dispose();
    //            _bitmap = null;
    //        }
    //    }
    //}




    //public static class LogitechG19LCDBitmap
    //{
    //    private static Bitmap _bitmap;

    //    private static readonly int[] _monoButtons = new[]
    //    {
    //        LogitechGSDK.LOGI_LCD_MONO_BUTTON_0,
    //        LogitechGSDK.LOGI_LCD_MONO_BUTTON_1,
    //        LogitechGSDK.LOGI_LCD_MONO_BUTTON_2,
    //        LogitechGSDK.LOGI_LCD_MONO_BUTTON_3
    //    };



    //    private static readonly SynchronizationContext _context;

    //    public static Bitmap Bitmap
    //    {
    //        get
    //        {
    //            if (_bitmap == null)
    //                _bitmap = new Bitmap(320, 240, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

    //            return _bitmap;
    //        }
    //    }

    //    static LogitechG19LCDBitmap()
    //    {
    //        var result = LogitechGSDK.LogiLcdInit(Assembly.GetEntryAssembly().GetName().FullName, LogitechGSDK.LOGI_LCD_TYPE_COLOR);
    //        if (!result)
    //            throw new Exception("Logitech G19 LCD is not found.");

    //        _context = SynchronizationContext.Current;

    //        Task.Run(() =>
    //        {
    //            while (true)
    //            {
    //                for (int i = 0; i < _monoButtons.Length; i++)
    //                    if (LogitechGSDK.LogiLcdIsButtonPressed(_monoButtons[i]))
    //                        switch (_monoButtons[i])
    //                        {
    //                            case (LogitechGSDK.LOGI_LCD_MONO_BUTTON_0):

    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_MONO_BUTTON_1):
    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_MONO_BUTTON_2):
    //                                break;
    //                            case (LogitechGSDK.LOGI_LCD_MONO_BUTTON_3):
    //                                break;
    //                        }


    //                Thread.Sleep(10);
    //            }
    //        });

    //    }

    //    public event EventHandler MenuButtonPressed;

    //    public event EventHandler MenuButtonPressed;


    //    public static int Width { get; } = 320;
    //    public static int Height { get; } = 240;



    //    public static LCDCanvas LcdBitmap() =>
    //        new LCDCanvas();

    //    public static void Update()
    //    {
    //        var data = _bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    //        try
    //        {
    //            // copy pixels..
    //            LogitechGSDK.LogiLcdColorSetBackground(data.Scan0);
    //        }
    //        finally
    //        {
    //            Bitmap.UnlockBits(data);
    //            LogitechGSDK.LogiLcdUpdate();
    //        }
    //    }

    //    public static void Free()
    //    {
    //        if (_bitmap != null)
    //        {
    //            _bitmap.Dispose();
    //            _bitmap = null;
    //        }
    //    }
    //}
}