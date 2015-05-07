using System.Reflection;
using Nest.Resolvers;
using Nest.Tests.MockData.Domain;
using NUnit.Framework;

namespace Nest.Tests.Unit.Search.Aggregations.Terms
{
	[TestFixture]
	public class TermAggregationTests : BaseJsonTests
	{
		[Test]
		public void TermAggregationOldOrder()
		{
			var s = new TermsAggregationDescriptor<ElasticsearchProject>()
				.CollectMode(TermsAggregationCollectMode.BreadthFirst)
				.ExecutionHint(TermsAggregationExecutionHint.GlobalOrdinalsLowCardinality)
				.Field(p => p.Country)
				.OrderAscending("_count")
				.MinimumDocumentCount(1);

			this.JsonEquals(s, MethodBase.GetCurrentMethod());
		}

		[Test]
		public void TermAggregationOrder()
		{
			var s = new TermsAggregationDescriptor<ElasticsearchProject>()
				.CollectMode(TermsAggregationCollectMode.BreadthFirst)
				.ExecutionHint(TermsAggregationExecutionHint.GlobalOrdinalsLowCardinality)
				.Field(p => p.Country)
				.Order(new TermsAggregationOrder
				{
					{"myotheragg>metric.avg", SortOrder.Ascending},
					{"_count", SortOrder.Descending}
				})
				.MinimumDocumentCount(1);

			this.JsonEquals(s, MethodBase.GetCurrentMethod());
		}

		[Test]
		public void TermAggregationWithFluentOrder()
		{
			var s = new TermsAggregationDescriptor<ElasticsearchProject>()
				.CollectMode(TermsAggregationCollectMode.BreadthFirst)
				.ExecutionHint(TermsAggregationExecutionHint.GlobalOrdinalsLowCardinality)
				.Field(p => p.Country)
				.Order(o => o
					.AggregationValue("myotheragg>metric.avg", SortOrder.Ascending)
					.Alphabetically(SortOrder.Ascending)
					.DocumentCount(SortOrder.Ascending)
				)
				.MinimumDocumentCount(1);

			this.JsonEquals(s, MethodBase.GetCurrentMethod());
		}

	}
}
