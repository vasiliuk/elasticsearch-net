namespace System.Net
{
    public class ServicePoint
    {
        public int ConnectionLimit { get; internal set; }
        public bool Expect100Continue { get; internal set; }
        public bool UseNagleAlgorithm { get; internal set; }

        internal void SetTcpKeepAlive(bool v, int value1, int value2)
        {
            throw new NotImplementedException();
        }
    }
}