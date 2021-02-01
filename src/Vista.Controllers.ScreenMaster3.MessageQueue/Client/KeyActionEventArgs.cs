using System;
using System.Collections.Generic;
using System.Text;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Client
{
    public delegate void KeyActionEventHandler(object sender, KeyActionEventArgs e);

    public class KeyActionEventArgs : EventArgs
    {
        public int KeyIndex { get; set; }
        public bool IsPressed { get; set; }

        public KeyActionEventArgs()
        {

        }

        public KeyActionEventArgs(int keyIndex, bool isPressed)
        {
            this.KeyIndex = keyIndex;
            this.IsPressed = isPressed;
        }
    }
}
