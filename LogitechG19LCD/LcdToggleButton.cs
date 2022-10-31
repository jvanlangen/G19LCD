using System;

namespace G19LCD
{
    struct LcdToggleButton
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
}