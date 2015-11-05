﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
//using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net.Connection.Configuration;
using Elasticsearch.Net.Providers;
using PurifyNet;

namespace Elasticsearch.Net.Connection
{
	public class HttpConnection : IConnection
	{
		const int BUFFER_SIZE = 1024;

		protected IConnectionConfigurationValues ConnectionSettings { get; set; }
		private readonly Semaphore _resourceLock;

		public TransportAddressScheme? AddressScheme { get; private set; }

		static HttpConnection()
		{
			//ServicePointManager.SetTcpKeepAlive(true, 2000, 2000);

			//WebException's GetResponse is limitted to 65kb by default.
			//Elasticsearch can be alot more chatty then that when dumping exceptions
			//On error responses, so lets up the ante.

			//Not available under mono
			if (Type.GetType("Mono.Runtime") == null)
				HttpWebRequest.DefaultMaximumErrorResponseLength = -1;
		}

		public HttpConnection(IConnectionConfigurationValues settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			if (settings.ConnectionPool.UsingSsl)
				this.AddressScheme = TransportAddressScheme.Https;

			this.ConnectionSettings = settings;
			if (settings.MaximumAsyncConnections > 0)
			{
				var semaphore = Math.Max(1, settings.MaximumAsyncConnections);
				this._resourceLock = new Semaphore(semaphore, semaphore);
			}
		}

		public virtual ElasticsearchResponse<Stream> GetSync(Uri uri, IRequestConfiguration requestSpecificConfig = null)
		{
			return this.HeaderOnlyRequest(uri, "GET", requestSpecificConfig);
		}

		public virtual ElasticsearchResponse<Stream> HeadSync(Uri uri, IRequestConfiguration requestSpecificConfig = null)
		{
			return this.HeaderOnlyRequest(uri, "HEAD", requestSpecificConfig);
		}

		public virtual ElasticsearchResponse<Stream> PostSync(Uri uri, byte[] data, IRequestConfiguration requestSpecificConfig = null)
		{
			return this.BodyRequest(uri, data, "POST", requestSpecificConfig);
		}
		
		public virtual ElasticsearchResponse<Stream> PutSync(Uri uri, byte[] data, IRequestConfiguration requestSpecificConfig = null)
		{
			return this.BodyRequest(uri, data, "PUT", requestSpecificConfig);
		}
		
		public virtual ElasticsearchResponse<Stream> DeleteSync(Uri uri, IRequestConfiguration requestSpecificConfig = null)
		{
			return this.HeaderOnlyRequest(uri, "DELETE", requestSpecificConfig);
		}
		
		public virtual ElasticsearchResponse<Stream> DeleteSync(Uri uri, byte[] data, IRequestConfiguration requestSpecificConfig = null)
		{
			return this.BodyRequest(uri, data, "DELETE", requestSpecificConfig);
		}

		private ElasticsearchResponse<Stream> HeaderOnlyRequest(Uri uri, string method, IRequestConfiguration requestSpecificConfig)
		{
			var r = this.CreateHttpWebRequest(uri, method, null, requestSpecificConfig);
			return this.DoSynchronousRequest(r, requestSpecificConfig: requestSpecificConfig);
		}

		private ElasticsearchResponse<Stream> BodyRequest(Uri uri, byte[] data, string method, IRequestConfiguration requestSpecificConfig)
		{
			var r = this.CreateHttpWebRequest(uri, method, data, requestSpecificConfig);
			return this.DoSynchronousRequest(r, data, requestSpecificConfig);
		}

		public virtual Task<ElasticsearchResponse<Stream>> Get(Uri uri, IRequestConfiguration requestSpecificConfig = null)
		{
			var r = this.CreateHttpWebRequest(uri, "GET", null, requestSpecificConfig);
			return this.DoAsyncRequest(r, requestSpecificConfig: requestSpecificConfig);
		}
		
		public virtual Task<ElasticsearchResponse<Stream>> Head(Uri uri, IRequestConfiguration requestSpecificConfig = null)
		{
			var r = this.CreateHttpWebRequest(uri, "HEAD", null, requestSpecificConfig);
			return this.DoAsyncRequest(r, requestSpecificConfig: requestSpecificConfig);
		}
		
		public virtual Task<ElasticsearchResponse<Stream>> Post(Uri uri, byte[] data, IRequestConfiguration requestSpecificConfig = null)
		{
			var r = this.CreateHttpWebRequest(uri, "POST", data, requestSpecificConfig);
			return this.DoAsyncRequest(r, data, requestSpecificConfig: requestSpecificConfig);
		}

		public virtual Task<ElasticsearchResponse<Stream>> Put(Uri uri, byte[] data, IRequestConfiguration requestSpecificConfig = null)
		{
			var r = this.CreateHttpWebRequest(uri, "PUT", data, requestSpecificConfig);
			return this.DoAsyncRequest(r, data, requestSpecificConfig: requestSpecificConfig);
		}

		public virtual Task<ElasticsearchResponse<Stream>> Delete(Uri uri, byte[] data, IRequestConfiguration requestSpecificConfig = null)
		{
			var r = this.CreateHttpWebRequest(uri, "DELETE", data, requestSpecificConfig);
			return this.DoAsyncRequest(r, data, requestSpecificConfig: requestSpecificConfig);
		}
		
		public virtual Task<ElasticsearchResponse<Stream>> Delete(Uri uri, IRequestConfiguration requestSpecificConfig = null)
		{
			var r = this.CreateHttpWebRequest(uri, "DELETE", null, requestSpecificConfig);
			return this.DoAsyncRequest(r, requestSpecificConfig: requestSpecificConfig);
		}

		private static void ThreadTimeoutCallback(object state, bool timedOut)
		{
			if (timedOut)
			{
				HttpWebRequest request = state as HttpWebRequest;
				if (request != null)
				{
					request.Abort();
				}
			}
		}

		protected virtual void AlterServicePoint(ServicePoint requestServicePoint)
		{
			requestServicePoint.UseNagleAlgorithm = false;
			requestServicePoint.Expect100Continue = false;
			requestServicePoint.ConnectionLimit = 10000;
			//looking at http://referencesource.microsoft.com/#System/net/System/Net/ServicePoint.cs
			//this method only sets internal values and wont actually cause timers and such to be reset
			//So it should be idempotent if called with the same parameters
			if (this.ConnectionSettings.KeepAliveTime.HasValue && this.ConnectionSettings.KeepAliveInterval.HasValue)
				requestServicePoint.SetTcpKeepAlive(true, this.ConnectionSettings.KeepAliveTime.Value, this.ConnectionSettings.KeepAliveInterval.Value);
		}

		protected virtual HttpWebRequest CreateHttpWebRequest(Uri uri, string method, byte[] data, IRequestConfiguration requestSpecificConfig)
		{
			var request = this.CreateWebRequest(uri, method, data, requestSpecificConfig);
			this.SetBasicAuthenticationIfNeeded(uri, request, requestSpecificConfig);
			this.SetProxyIfNeeded(request);
			this.AlterServicePoint(request.ServicePoint);
			return request;
		}

		private void SetProxyIfNeeded(HttpWebRequest myReq)
		{
			if (!string.IsNullOrEmpty(this.ConnectionSettings.ProxyAddress))
			{
				var proxy = new WebProxy();
				var uri = new Uri(this.ConnectionSettings.ProxyAddress);
				var credentials = new NetworkCredential(this.ConnectionSettings.ProxyUsername, this.ConnectionSettings.ProxyPassword);
				proxy.Address = uri;
				proxy.Credentials = credentials;
				myReq.Proxy = proxy;
			}

			if (this.ConnectionSettings.DisableAutomaticProxyDetection)
			{
				myReq.Proxy = null;
			}
		}

		private void SetBasicAuthenticationIfNeeded(Uri uri, HttpWebRequest request, IRequestConfiguration requestSpecificConfig)
		{
			// Basic auth credentials take the following precedence (highest -> lowest):
			// 1 - Specified on the request (highest precedence)
			// 2 - Specified at the global IConnectionSettings level
			// 3 - Specified with the URI (lowest precedence)

			var userInfo = Uri.UnescapeDataString(uri.UserInfo);

			if (this.ConnectionSettings.BasicAuthorizationCredentials != null)
				userInfo = this.ConnectionSettings.BasicAuthorizationCredentials.ToString();

			if (requestSpecificConfig != null && requestSpecificConfig.BasicAuthorizationCredentials != null)
				userInfo = requestSpecificConfig.BasicAuthorizationCredentials.ToString();

			if (!userInfo.IsNullOrEmpty())
				request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userInfo));
		}

		protected virtual HttpWebRequest CreateWebRequest(Uri uri, string method, byte[] data, IRequestConfiguration requestSpecificConfig)
		{
			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.Accept = "application/json";
			request.ContentType = "application/json";
			request.MaximumResponseHeadersLength = -1;
			request.Pipelined = this.ConnectionSettings.HttpPipeliningEnabled
				|| (requestSpecificConfig != null && requestSpecificConfig.EnableHttpPipelining);

			if (this.ConnectionSettings.EnableCompressedResponses
				|| this.ConnectionSettings.EnableHttpCompression)
			{
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				request.Headers.Add("Accept-Encoding", "gzip,deflate");
				if (this.ConnectionSettings.EnableHttpCompression)
					request.Headers.Add("Content-Encoding", "gzip");
			}

			if (requestSpecificConfig != null && !string.IsNullOrWhiteSpace(requestSpecificConfig.ContentType))
			{
				request.Accept = requestSpecificConfig.ContentType;
				request.ContentType = requestSpecificConfig.ContentType;
			}

			var timeout = GetRequestTimeout(requestSpecificConfig);
			request.Timeout = timeout;
			request.ReadWriteTimeout = timeout;
			request.Method = method;

			//WebRequest won't send Content-Length: 0 for empty bodies
			//which goes against RFC's and might break i.e IIS when used as a proxy.
			//see: https://github.com/elasticsearch/elasticsearch-net/issues/562
			var m = method.ToLowerInvariant();
			if (m != "head" && m != "get" && (data == null || data.Length == 0))
				request.ContentLength = 0;

			return request;
		}

		protected virtual ElasticsearchResponse<Stream> DoSynchronousRequest(HttpWebRequest request, byte[] data = null, IRequestConfiguration requestSpecificConfig = null)
		{
			var path = request.RequestUri.ToString();
			var method = request.Method;

			if (data != null)
			{

				using (var r = request.GetRequestStream())
				{
					if (this.ConnectionSettings.EnableHttpCompression)
						using (var zipStream = new GZipStream(r, CompressionMode.Compress))
							zipStream.Write(data, 0, data.Length);
					else 
						r.Write(data, 0, data.Length);
				}
			}
			try
			{
				//http://msdn.microsoft.com/en-us/library/system.net.httpwebresponse.getresponsestream.aspx
				//Either the stream or the response object needs to be closed but not both although it won't
				//throw any errors if both are closed atleast one of them has to be Closed.
				//Since we expose the stream we let closing the stream determining when to close the connection
				var response = (HttpWebResponse)request.GetResponse();
				var responseStream = response.GetResponseStream();
				return WebToElasticsearchResponse(data, responseStream, response, method, path);
			}
			catch (WebException webException)
			{
				return HandleWebException(data, webException, method, path);
			}
		}

		private ElasticsearchResponse<Stream> HandleWebException(byte[] data, WebException webException, string method, string path)
		{
			ElasticsearchResponse<Stream> cs = null;
			var httpEx = webException.Response as HttpWebResponse;
			if (httpEx != null)
			{
				//StreamReader ms = new StreamReader(httpEx.GetResponseStream());
				//var response = ms.ReadToEnd();
				cs = WebToElasticsearchResponse(data, httpEx.GetResponseStream(), httpEx, method, path);
				return cs;
			}
			cs = ElasticsearchResponse<Stream>.CreateError(this.ConnectionSettings, webException, method, path, data);
			return cs;
		}

		private ElasticsearchResponse<Stream> WebToElasticsearchResponse(byte[] data, Stream responseStream, HttpWebResponse response, string method, string path)
		{
			ElasticsearchResponse<Stream> cs = ElasticsearchResponse<Stream>.Create(this.ConnectionSettings, (int)response.StatusCode, method, path, data);
			cs.Response = responseStream;
			return cs;
		}

		protected virtual Task<ElasticsearchResponse<Stream>> DoAsyncRequest(HttpWebRequest request, byte[] data = null, IRequestConfiguration requestSpecificConfig = null)
		{
			var tcs = new TaskCompletionSource<ElasticsearchResponse<Stream>>();
			if (this.ConnectionSettings.MaximumAsyncConnections <= 0
			  || this._resourceLock == null)
				return this.CreateIterateTask(request, data, requestSpecificConfig, tcs);

			var timeout = GetRequestTimeout(requestSpecificConfig);
			var path = request.RequestUri.ToString();
			var method = request.Method;
			if (!this._resourceLock.WaitOne(timeout))
			{
				var m = "Could not start the operation before the timeout of " + timeout +
				  "ms completed while waiting for the semaphore";
				var cs = ElasticsearchResponse<Stream>.CreateError(this.ConnectionSettings, new TimeoutException(m), method, path, data);
				tcs.SetResult(cs);
				return tcs.Task;
			}
			try
			{
				return this.CreateIterateTask(request, data, requestSpecificConfig, tcs);
			}
			finally
			{
				this._resourceLock.Release();
			}
		}

		private Task<ElasticsearchResponse<Stream>> CreateIterateTask(HttpWebRequest request, byte[] data, IRequestConfiguration requestSpecificConfig, TaskCompletionSource<ElasticsearchResponse<Stream>> tcs)
		{
			this.Iterate(request, data, this._AsyncSteps(request, tcs, data, requestSpecificConfig), tcs);
			return tcs.Task;
		}

		private IEnumerable<Task> _AsyncSteps(HttpWebRequest request, TaskCompletionSource<ElasticsearchResponse<Stream>> tcs, byte[] data, IRequestConfiguration requestSpecificConfig)
		{
			var timeout = GetRequestTimeout(requestSpecificConfig);

            if (data != null)
            {
#if NETFXCORE
                var getRequestStream = Task.Run(async () =>
                {
                    using (var cts = new CancellationTokenSource(timeout))
                    {
                        var token = cts.Token;
                        token.Register(() => ThreadTimeoutCallback(request, true));
                        try
                        {
                            return await request.GetRequestStreamAsync(cts.Token);
                        }
                        catch (TaskCanceledException) { throw new TimeoutException(); } // TODO: probably not needed
                    }
                });

                yield return getRequestStream;
#else
                var getRequestStream = Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null);
				ThreadPool.RegisterWaitForSingleObject((getRequestStream as IAsyncResult).AsyncWaitHandle, ThreadTimeoutCallback, request, timeout, true);
                yield return getRequestStream;
#endif


                var requestStream = getRequestStream.Result;
				try
				{
					if (this.ConnectionSettings.EnableHttpCompression)
					{
						using (var zipStream = new GZipStream(requestStream, CompressionMode.Compress))
						{
#if NETFXCORE
                            yield return zipStream.WriteAsync(data, 0, data.Length);
#else
                            var writeToRequestStream = Task.Factory.FromAsync(zipStream.BeginWrite, zipStream.EndWrite, data, 0,
								data.Length, null);
							yield return writeToRequestStream;
#endif
						}
					}
					else
					{
#if NETFXCORE
                        yield return requestStream.WriteAsync(data, 0, data.Length);
#else
                        var writeToRequestStream = Task.Factory.FromAsync(requestStream.BeginWrite, requestStream.EndWrite, data, 0,
							data.Length, null);
						yield return writeToRequestStream;
#endif
					}
				}
				finally
				{
					requestStream.Close();
				}
			}

#if NETFXCORE
            var getResponse = Task.Run(async () =>
            {
                using (var cts = new CancellationTokenSource(timeout))
                {
                    var token = cts.Token;
                    token.Register(() => ThreadTimeoutCallback(request, true));
                    try
                    {
                        return await request.GetResponseAsync(cts.Token);
                    }
                    catch (TaskCanceledException) { throw new TimeoutException(); } // TODO: probably not needed
                }
            });
#else
            // Get the response
            var getResponse = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
			ThreadPool.RegisterWaitForSingleObject((getResponse as IAsyncResult).AsyncWaitHandle, ThreadTimeoutCallback, request, timeout, true);
			yield return getResponse;
#endif

            var path = request.RequestUri.ToString();
			var method = request.Method;

			//http://msdn.microsoft.com/en-us/library/system.net.httpwebresponse.getresponsestream.aspx
			//Either the stream or the response object needs to be closed but not both (although it won't)
			//throw any errors if both are closed atleast one of them has to be Closed.
			//Since we expose the stream we let closing the stream determining when to close the connection
			var response = (HttpWebResponse)getResponse.Result;
			var responseStream = response.GetResponseStream();
			var cs = ElasticsearchResponse<Stream>.Create(this.ConnectionSettings, (int)response.StatusCode, method, path, data);
			cs.Response = responseStream;
			tcs.TrySetResult(cs);
		}

		private void Iterate(HttpWebRequest request, byte[] data, IEnumerable<Task> asyncIterator, TaskCompletionSource<ElasticsearchResponse<Stream>> tcs)
		{
			var enumerator = asyncIterator.GetEnumerator();
			Action<Task> recursiveBody = null;
			recursiveBody = completedTask =>
			{
				if (completedTask != null && completedTask.IsFaulted)
				{
					//none of the individual steps in _AsyncSteps run in parallel for 1 request
					//as this would be impossible we can assume Aggregate Exception.InnerException
					var exception = completedTask.Exception.InnerException;

					//cleanly exit from exceptions in stages if the exception is a webexception
					if (exception is WebException)
					{
						var path = request.RequestUri.ToString();
						var method = request.Method;
						var response = this.HandleWebException(data, exception as WebException, method, path);
						tcs.SetResult(response);
					}
					else
						tcs.TrySetException(exception);
					enumerator.Dispose();
				}
				else if (enumerator.MoveNext())
				{
					enumerator.Current.ContinueWith(recursiveBody, TaskContinuationOptions.ExecuteSynchronously);
				}
				else enumerator.Dispose();
			};
			recursiveBody(null);
		}

		private int GetRequestTimeout(IRequestConfiguration requestConfiguration)
		{
			if (requestConfiguration != null && requestConfiguration.RequestTimeout.HasValue)
				return requestConfiguration.RequestTimeout.Value;

			return this.ConnectionSettings.Timeout;
		}
	}
}
