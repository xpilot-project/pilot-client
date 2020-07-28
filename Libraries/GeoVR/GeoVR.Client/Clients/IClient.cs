using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public interface IClient
    {
        //ClientStatistics ClientStatistics { get; }
        //ClientData ClientData { get; }

        //void SetupTransceivers(int numberOfTransceivers, string callsign);
        //bool PTT(int transceiverID, bool ptt);
        //void Frequency(int transceiverID, string frequency);
        //void Position(int transceiverID, double latDeg, double lonDeg, double groundAltM);

        bool Started { get; }
        DateTime StartDateTimeUtc { get; }

        void Stop();            //Can't have Start - different parameters
    }
}
