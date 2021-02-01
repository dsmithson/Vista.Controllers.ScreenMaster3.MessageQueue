using System;
using System.Collections.Generic;
using System.Text;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Client
{
    public delegate void RotaryActionEventHandler(object sender, RotaryActionEventArgs e);

    public class RotaryActionEventArgs : EventArgs
    {
        public int RotaryIndex { get; set; }
        public int RotaryOffset { get; set; }

        public RotaryActionEventArgs()
        {

        }

        public RotaryActionEventArgs(int rotaryIndex, int rotaryOffset)
        {
            this.RotaryIndex = rotaryIndex;
            this.RotaryOffset = rotaryOffset;
        }
    }
}
