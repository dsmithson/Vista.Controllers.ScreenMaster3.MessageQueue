using System;
using System.Collections.Generic;
using System.Text;

namespace Vista.Controllers.ScreenMaster3.MessageQueue
{
    public static class RoutingAddressMap
    {
        public const string Exchange = "Vista.ScreenMaster";

        public const string ExchangeType = "topic";


        public const string KeyActionRoutingKey = "actions.keyaction";

        public const string JoystickActionRoutingKey = "actions.joystick";

        public const string RotaryActionRoutingKey = "actions.rotary";

        public const string TBarActionRoutingKey = "actions.tbar";


        public const string LampCommandRoutingKey = "commands.lamp";

        public const string QuickKeyCommandRoutingKey = "commands.quickkey";
    }
}
