using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net
{
    public class HttpWebRequest
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

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public Stream GetRequestStream()
        {
            throw new NotImplementedException();
        }

        Func<Stream> _GetRequestStream;

        public IAsyncResult BeginGetRequestStream(AsyncCallback callback, object @object)
        {
            return _GetRequestStream.BeginInvoke(callback, @object);
        }

        public Stream EndGetRequestStream(IAsyncResult result)
        {
            return _GetRequestStream.EndInvoke(result);
        }

        public HttpWebResponse GetResponse()
        {
            throw new NotImplementedException();
        }

        internal IAsyncResult BeginGetResponse(AsyncCallback callback, object @object)
        {
            throw new NotImplementedException();
        }

        internal HttpWebResponse EndGetResponse(IAsyncResult obj)
        {
            throw new NotImplementedException();
        }
    }
}
