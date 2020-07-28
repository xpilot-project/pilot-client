using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoVR.Shared
{
    public class PostUserRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid NetworkVersion { get; set; }
        public string Client { get; set; }
    }
}
