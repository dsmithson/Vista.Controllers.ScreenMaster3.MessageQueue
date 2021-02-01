using System;
using System.Collections.Generic;
using System.Text;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Client
{
    public delegate void TBarActionEventHandler(object sender, TBarActionEventArgs e);

    public class TBarActionEventArgs
    {
        public int TBarPosition { get; set; }

        public TBarActionEventArgs()
        {

        }

        public TBarActionEventArgs(int tbarPosition)
        {
            this.TBarPosition = tbarPosition;
        }
    }
}
