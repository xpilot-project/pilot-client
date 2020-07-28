using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public enum DirectMode
    {
        Disabled = 0,
        AllowForWhitelist = 1,
        AllowAll = 2
    }

    [MessagePackObject]
    public class DirectConfigurationDto : IMsgPack
    {
        [Key(0)]
        public DirectMode Mode { get; set; }
        [Key(1)]
        public List<string> Whitelist { get; set; }
    }
}
