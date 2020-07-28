using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Shared
{
    public class CallsignOnTransceiverNames
    {
        public string Callsign { get; set; }
        public List<string> TransceiverNames { get; set; }

        public override string ToString()
        {
            return Callsign + " [" + string.Join(", ", TransceiverNames) + "]";
        }
    }
}
