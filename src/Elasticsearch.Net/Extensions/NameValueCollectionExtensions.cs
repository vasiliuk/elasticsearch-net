﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Elasticsearch.Net.Serialization;

namespace Elasticsearch.Net
{
	internal static class NameValueCollectionExtensions
	{
		internal static void CopyKeyValues(this NameValueCollection source, NameValueCollection dest)
		{
			foreach (var key in source.AllKeys)
			{
                //if (dest[key] != null) throw new ApplicationException(string.Format("Attempted to add duplicate key '{0}'", key));
                
                // TODO: check this exception can be rpobably uncaught
                if (dest[key] != null) throw new ArgumentException(string.Format("Attempted to add duplicate key '{0}'", key));

                dest.Add(key, source[key]);
			}
		}

		internal static string ToQueryString(this NameValueCollection self, string prefix = "?")
		{
			if (self == null)
				return null;

			if (self.AllKeys.Length == 0) return string.Empty;

            var pieces =
#if NETFXCORE
                self.AllKeys.Select(
#else
                Array.ConvertAll(self.AllKeys,
#endif
                key => string.Format("{0}={1}", Encode(key), Encode(self[key])));

            return prefix + string.Join("&", pieces);
		}
		private static string Encode(string s)
		{
			return s == null ? null : Uri.EscapeDataString(s);
		}

		internal static NameValueCollection ToNameValueCollection(this IDictionary<string, object> dict, IElasticsearchSerializer stringifier)
		{
			stringifier.ThrowIfNull("stringifier");
			if (dict == null || dict.Count < 0)
				return null;
			
			var nv = new NameValueCollection();
			foreach (var kv in dict.Where(kv => !kv.Key.IsNullOrEmpty()))
			{
				nv.Add(kv.Key, stringifier.Stringify(kv.Value));
			}
			return nv;
		}
	}
}
