using System;
using System.Collections.Generic;

namespace Didimo.Networking
{
    public interface IRequestHeader
    {
        bool TryGetHeaders(Uri uri, out Dictionary<string, string> headers);
    }
}