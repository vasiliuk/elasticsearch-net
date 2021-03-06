﻿using Elasticsearch.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nest
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public interface ICatCountRequest : IRequest<CatCountRequestParameters>
	{
	}

	public partial class CatCountRequest : BasePathRequest<CatCountRequestParameters>, ICatCountRequest
	{
		protected override void UpdatePathInfo(IConnectionSettingsValues settings, ElasticsearchPathInfo<CatCountRequestParameters> pathInfo)
		{
			CatRequestPathInfo.Update(pathInfo);
		}
	}

	public partial class CatCountDescriptor : BasePathDescriptor<CatCountDescriptor, CatCountRequestParameters>, ICatCountRequest
	{
		protected override void UpdatePathInfo(IConnectionSettingsValues settings, ElasticsearchPathInfo<CatCountRequestParameters> pathInfo)
		{
			CatRequestPathInfo.Update(pathInfo);
		}
	}
}
