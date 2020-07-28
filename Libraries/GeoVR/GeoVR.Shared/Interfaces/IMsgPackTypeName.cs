using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public interface IMsgPackTypeName //This is a promise that the developer has implemented [MessagePackObject] and will implement TypeName & TypeNameConst
    {
        string TypeName { get; }
    }
}
