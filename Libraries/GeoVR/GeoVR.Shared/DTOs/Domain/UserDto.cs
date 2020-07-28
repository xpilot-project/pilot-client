using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class UserDto
    {
        public Guid ID { get; set; }
        public string Username { get; set; }
        public bool Enabled { get; set; }
    }
}
