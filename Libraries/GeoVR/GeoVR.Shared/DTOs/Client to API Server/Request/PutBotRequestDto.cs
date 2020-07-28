using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class PutBotRequestDto
    {
        public List<TransceiverDto> Transceivers { get; set; }
        public TimeSpan Interval { get; set; }
        public List<byte[]> OpusData { get; set; }
    }
}
