using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class NotificationEventArgs : EventArgs
    {
        public string Message { get; set; }

        public NotificationEventArgs(string message)
        {
            Message = message;
        }
    }
}
