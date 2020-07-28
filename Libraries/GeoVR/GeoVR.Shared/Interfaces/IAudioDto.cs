using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public interface IAudioDto
    {
        string Callsign { get; set; }           // Callsign that audio originates from
        uint SequenceCounter { get; set; }      // Receiver optionally uses this in reordering algorithm/gap detection
        byte[] Audio { get; set; }              // Opus compressed audio
        bool LastPacket { get; set; }           // Used to indicate to receiver that the sender has stopped sending
    }
}
