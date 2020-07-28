using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared.DTOs
{
    public class PutUserEnabledDto
    {
        public string Username { get; set; }
        public bool Enabled { get; set; }
    }
}
