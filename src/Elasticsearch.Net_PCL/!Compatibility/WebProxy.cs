namespace System.Net
{
    public class WebProxy
    {
        public WebProxy()
        {
        }

        public Uri Address { get; internal set; }
        public NetworkCredential Credentials { get; internal set; }
    }
}