using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Shared
{
    public class DisconnectedEventArgs
    {
        public string Reason { get; set; }
        public bool AutoReconnect { get; set; }
    }
}
