using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vatsim.Xpilot.Simulator;

namespace XPilot.PilotClient.Core.Events
{
    public class RadioStackStateChanged : EventArgs
    {
        public UserAircraftRadioStack RadioStack { get; set; }
        public RadioStackStateChanged(UserAircraftRadioStack radioStack)
        {
            RadioStack = radioStack;
        }
    }
}