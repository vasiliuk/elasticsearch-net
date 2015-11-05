using System.IO;

namespace System.Net
{
    public class HttpWebResponse : WebResponse
    {
        public HttpStatusCode StatusCode { get; internal set; }

        public Stream GetResponseStream()
        {
            throw new NotImplementedException();
        }
    }
}