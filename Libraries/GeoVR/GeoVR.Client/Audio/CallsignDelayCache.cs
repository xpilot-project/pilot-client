using System;
using System.Collections.Generic;

namespace GeoVR.Client
{
    public class CallsignDelayCache
    {
        private readonly int delayDefault = 60;
        private readonly int delayMin = 40;
        private readonly int delayIncrement = 20;
        private readonly int delayMax = 300;

        //Singleton Pattern
        private static readonly Lazy<CallsignDelayCache> lazy = new Lazy<CallsignDelayCache>(() => new CallsignDelayCache());

        public static CallsignDelayCache Instance { get { return lazy.Value; } }

        private CallsignDelayCache() { }
        //End of Singleton Pattern

        private Dictionary<string, int> delayCache = new Dictionary<string, int>();
        private Dictionary<string, int> successfulTransmissionsCache = new Dictionary<string, int>();

        public void Initialise(string callsign)
        {
            if (!delayCache.ContainsKey(callsign))
                delayCache[callsign] = delayDefault;

            if (!successfulTransmissionsCache.ContainsKey(callsign))
                successfulTransmissionsCache[callsign] = 0;
        }

        public int Get(string callsign)
        {
            return delayCache[callsign];
        }

        public void Underflow(string callsign)
        {
            if (!successfulTransmissionsCache.ContainsKey(callsign))
                return;

            successfulTransmissionsCache[callsign] = 0;
            IncreaseDelayMs(callsign);
        }

        public void Success(string callsign)
        {
            if (!successfulTransmissionsCache.ContainsKey(callsign))
                return;

            successfulTransmissionsCache[callsign]++;
            if (successfulTransmissionsCache[callsign] > 5)
            {
                DecreaseDelayMs(callsign);
                successfulTransmissionsCache[callsign] = 0;
            }
        }

        private void IncreaseDelayMs(string callsign)
        {
            if (!delayCache.ContainsKey(callsign))
                return;

            delayCache[callsign] += delayIncrement;
            if (delayCache[callsign] > delayMax)
            {
                delayCache[callsign] = delayMax;
            }
        }

        private void DecreaseDelayMs(string callsign)
        {
            if (!delayCache.ContainsKey(callsign))
                return;

            delayCache[callsign] -= delayIncrement;
            if (delayCache[callsign] < delayMin)
            {
                delayCache[callsign] = delayMin;
            }
        }
    }
}
