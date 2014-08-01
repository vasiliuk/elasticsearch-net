﻿using System.Threading.Tasks;

namespace Nest
{
	/// <summary>
	/// Provides extension methods to provide a cleaner scoll API given a scollTime and scrollId
	/// </summary>
	public static class ExistsExtensions
	{
		public static IExistsResponse IndexExists(this IElasticClient client, string indexName)
		{
			return client.IndexExists(new IndexExistsRequest(indexName));
		}

		public static Task<IExistsResponse> IndexExistsAsync(this IElasticClient client, string indexName)
		{
			return client.IndexExistsAsync(new IndexExistsRequest(indexName));
		}

		public static IExistsResponse TypeExists(this IElasticClient client, string indexName, string typeName)
		{
			return client.TypeExists(new TypeExistsRequest(indexName, typeName));
		}

		public static Task<IExistsResponse> TypeExistsAsync(this IElasticClient client, string indexName, string typeName)
		{
			return client.TypeExistsAsync(new TypeExistsRequest(indexName, typeName));
		}

	}
}