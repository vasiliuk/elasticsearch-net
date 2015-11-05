using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if NETFXCORE
namespace System.Net
{
    public class HttpWebRequest : WebRequest
    {
        public static int DefaultMaximumErrorResponseLength { get; internal set; }
        public string Accept { get; internal set; }
        public DecompressionMethods AutomaticDecompression { get; internal set; }
        public int ContentLength { get; internal set; }
        public string ContentType { get; internal set; }
        public IDictionary<string, string> Headers { get; internal set; }
        public int MaximumResponseHeadersLength { get; internal set; }
        public string Method { get; internal set; }
        public bool Pipelined { get; internal set; }
        public WebProxy Proxy { get; internal set; }
        public int ReadWriteTimeout { get; internal set; }
        public Uri RequestUri { get; internal set; }
        public ServicePoint ServicePoint { get; internal set; }
        public int Timeout { get; internal set; }

        Uri uri;

        HttpClient activeHttpClient;

        internal HttpWebRequest(Uri uri)
        {
            this.uri = uri;
        }

        public void Abort()
        {
            var client = Interlocked.Exchange(ref activeHttpClient, null);
            client.Dispose();
        }

        public Stream GetRequestStream()
        {
            return GetRequestStreamAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public HttpWebResponse GetResponse()
        {
            return GetResponseAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        static Dictionary<string, HttpMethod> stringToHttpMethodMap = new Dictionary<string, HttpMethod>()
        {
            { "DELETE", HttpMethod.Delete},
            { "GET", HttpMethod.Get },
            { "HEAD", HttpMethod.Head },
            { "OPTIONS", HttpMethod.Options },
            { "POST", HttpMethod.Post },
            { "PUT", HttpMethod.Put },
            { "TRACE", HttpMethod.Trace }
        };


        internal Task<Stream> GetRequestStreamAsync(CancellationToken token)
        {
            //HttpMethod method;
            //if (!stringToHttpMethodMap.TryGetValue(this.Method, out method))
            //    throw new InvalidOperationException($"Unknown method :{this.Method}");

            //var httpReqMessage = new HttpRequestMessage()
            //var httpClient = new HttpClient().SendAsync()

            throw new NotImplementedException();
        }

        internal Task<HttpWebResponse> GetResponseAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
#endif