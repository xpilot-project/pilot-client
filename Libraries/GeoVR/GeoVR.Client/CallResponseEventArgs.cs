using GeoVR.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class CallResponseEventArgs
    {
        public string Callsign { get; set; }
        public CallResponseEvent Event { get; set; }
    }
}
