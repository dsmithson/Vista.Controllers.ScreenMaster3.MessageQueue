using System;
using System.Collections.Generic;
using System.Text;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Client
{
    public delegate void JoyStickActionEventHandler(object sender, JoystickActionEventArgs e);

    public class JoystickActionEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public JoystickActionEventArgs()
        {

        }

        public JoystickActionEventArgs(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
