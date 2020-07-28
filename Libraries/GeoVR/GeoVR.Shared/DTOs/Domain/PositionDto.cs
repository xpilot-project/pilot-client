using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared.DTOs.Domain
{
    [MessagePackObject]
    class PositionDto
    {
        [Key(0)]
        public uint SequenceCounter { get; set; }   //4 bytes
        [Key(1)]
        public ushort MsCounter { get; set; }       //2 bytes goes from 0 - 59999 (1 minute)
        [Key(2)]
        public double LatDeg { get; set; }          //8 bytes
        [Key(3)]
        public double LonDeg { get; set; }          //8 bytes
        [Key(4)]
        public float HeightMslM { get; set; }       //4 bytes
        [Key(5)]
        public float HeightAglM { get; set; }       //4 bytes
        [Key(6)]
        public float PitchDeg { get; set; }         //4 bytes
        [Key(7)]
        public float BankDeg { get; set; }          //4 bytes
        [Key(8)]
        public float HeadingDeg { get; set; }       //4 bytes
    }   //42 bytes
}
