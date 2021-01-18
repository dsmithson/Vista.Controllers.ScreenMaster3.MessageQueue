using Spyder.Controllers.ScreenMaster3;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Messages
{
    public static class QuickKeyColorExtensions
    {
        public static LcdColor Convert(this QuickKeyColor color)
        {
            return color switch
            {
                QuickKeyColor.Off => LcdColor.Off,
                QuickKeyColor.Green => LcdColor.Green,
                QuickKeyColor.Red => LcdColor.Red,
                QuickKeyColor.Alternate => LcdColor.Alternate,
                QuickKeyColor.GreenRed => LcdColor.GreenRed,
                QuickKeyColor.RedGreen => LcdColor.RedGreen,
                _ => throw new NotSupportedException("Failed to convert from specified type"),
            };
        }
    }
}
