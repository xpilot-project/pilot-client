using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class TransceiverReceivingCallsignsChangedEventArgs
    {
        public ushort TransceiverID { get; private set; }
        public List<string> ReceivingCallsigns { get; private set; }

        public TransceiverReceivingCallsignsChangedEventArgs(ushort transceiverID, List<string> receivingCallsigns)
        {
            TransceiverID = transceiverID;
            ReceivingCallsigns = receivingCallsigns;
        }
    }
}
