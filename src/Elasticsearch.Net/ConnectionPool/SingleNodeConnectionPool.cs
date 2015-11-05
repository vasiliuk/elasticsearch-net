﻿using System;
using System.Collections.Generic;
using Elasticsearch.Net.Connection;

namespace Elasticsearch.Net.ConnectionPool
{
	public class SingleNodeConnectionPool : IConnectionPool
	{
		private readonly Uri _uri;
		public int MaxRetries { get { return 0;  } }

		public bool AcceptsUpdates { get { return false; } }
		
		//public bool UsingSsl { get { return _uri.Scheme == Uri.UriSchemeHttps; } }
        public bool UsingSsl { get { return "https".Equals(_uri.Scheme, StringComparison.OrdinalIgnoreCase); } }

        public bool SniffedOnStartup
		{
			get { return false; }
			set {  }
		}

		public SingleNodeConnectionPool(Uri uri)
		{
			//this makes sure that paths stay relative i.e if the root uri is:
			//http://my-saas-provider.com/instance
			if (!uri.OriginalString.EndsWith("/"))
				uri = new Uri(uri.OriginalString + "/");
			_uri = uri;
		}

		public Uri GetNext(int? initialSeed, out int seed, out bool shouldPingHint)
		{
			seed = 0;
			shouldPingHint = false;
			return _uri;
		}

		public void MarkDead(Uri uri, int? deadTimeout = null, int? maxDeadTimeout = null)
		{
		}

		public void MarkAlive(Uri uri)
		{
		}

		public void UpdateNodeList(IList<Uri> newClusterState, Uri sniffNode = null)
		{
		}

	}
}