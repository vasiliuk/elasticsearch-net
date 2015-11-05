using System;
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
using System.Net.Http;
using System.Net.Http.Headers;

namespace Elasticsearch.Net.Connection
{
    public class HttpClientConnection : IConnection
    {
        public HttpClientConnection()
        {

        }

        public HttpClientConnection(IConnectionConfigurationValues settings)
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

        protected IConnectionConfigurationValues ConnectionSettings { get; set; }
        private readonly Semaphore _resourceLock;

        public TransportAddressScheme? AddressScheme { get; private set; }

        public Task<ElasticsearchResponse<Stream>> Delete(Uri uri, IRequestConfiguration requestConfiguration = null)
        {
            return DoRequest(uri, HttpMethod.Delete, null, requestConfiguration);
        }

        public Task<ElasticsearchResponse<Stream>> Delete(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null)
        {
            return DoRequest(uri, HttpMethod.Delete, data, requestConfiguration);
        }

        public ElasticsearchResponse<Stream> DeleteSync(Uri uri, IRequestConfiguration requestConfiguration = null)
        {
            return Delete(uri, requestConfiguration)
                .GetAwaiter().GetResult(); // in case of deadlock use Task.Run
        }

        public ElasticsearchResponse<Stream> DeleteSync(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null)
        {
            return Delete(uri, data, requestConfiguration)
                .GetAwaiter().GetResult(); // in case of deadlock use Task.Run
        }

        public Task<ElasticsearchResponse<Stream>> Get(Uri uri, IRequestConfiguration requestConfiguration = null)
        {
            return DoRequest(uri, HttpMethod.Get, null, requestConfiguration);
        }

        public ElasticsearchResponse<Stream> GetSync(Uri uri, IRequestConfiguration requestConfiguration = null)
        {
            return Get(uri, requestConfiguration)
                .GetAwaiter().GetResult(); // in case of deadlock use Task.Run
        }

        public Task<ElasticsearchResponse<Stream>> Head(Uri uri, IRequestConfiguration requestConfiguration = null)
        {
            return DoRequest(uri, HttpMethod.Head, null, requestConfiguration);
        }

        public ElasticsearchResponse<Stream> HeadSync(Uri uri, IRequestConfiguration requestConfiguration = null)
        {
            return Head(uri, requestConfiguration)
                .GetAwaiter().GetResult(); // in case of deadlock use Task.Run
        }

        public Task<ElasticsearchResponse<Stream>> Post(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null)
        {
            return DoRequest(uri, HttpMethod.Post, data, requestConfiguration);
        }

        public ElasticsearchResponse<Stream> PostSync(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null)
        {
            return Post(uri, data, requestConfiguration)
                .GetAwaiter().GetResult(); // in case of deadlock use Task.Run
        }

        public Task<ElasticsearchResponse<Stream>> Put(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null)
        {
            return DoRequest(uri, HttpMethod.Put, data, requestConfiguration);
        }

        public ElasticsearchResponse<Stream> PutSync(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null)
        {
            return Post(uri, data, requestConfiguration)
                .GetAwaiter().GetResult(); // in case of deadlock use Task.Run
        }

        private async Task<ElasticsearchResponse<Stream>> DoRequest(Uri uri, HttpMethod method, byte[] data, IRequestConfiguration requestSpecificConfig)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new ByteArrayContent(data);

                var message = new HttpRequestMessage(method, uri)
                {
                    Content = content,
                };

                //message.Headers.Add("Accept", "application/json");
                message.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                message.Method = method;

                var timeout = requestSpecificConfig?.RequestTimeout ??
                    this.ConnectionSettings.Timeout;

                httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);

                var result = await httpClient.SendAsync(message);

                return await CreateElasticsearchResponse(data, result, method, uri.ToString());
            }
        }

        private async Task<ElasticsearchResponse<Stream>> CreateElasticsearchResponse(byte[] data, HttpResponseMessage response, HttpMethod method, string path)
        {
            Stream responseStream = await response.Content.ReadAsStreamAsync();
            ElasticsearchResponse<Stream> cs = ElasticsearchResponse<Stream>.Create(this.ConnectionSettings, (int)response.StatusCode, method.Method, path, data);
            cs.Response = responseStream;
            return cs;
        }
    }
}
