﻿using System;

namespace Nest
{
    public static class UriExtensions
    {
        public static string ToUrlAndOverridePath(this Uri uri, string path)
        {
            return "https".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase) ?
                string.Format("https://{0}{1}", uri.Host, path) :
                string.Format("http://{0}:{1}{2}", uri.Host, uri.Port, path);
        }
    }
}
