using System;
using System.Net;

namespace System.Net
{
    public class WebRequest
    {
        internal static HttpWebRequest Create(Uri uri)
        {
            if ("http".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase) ||
                "https".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
                return new HttpWebRequest(uri);

            throw new NotImplementedException(); // notsupportedexception ??
        }
    }
}