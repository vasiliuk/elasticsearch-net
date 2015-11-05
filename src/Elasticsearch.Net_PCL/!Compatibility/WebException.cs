using System;

namespace System.Net
{
    //
    // Summary:
    //     The exception that is thrown when an error occurs while accessing the network
    //     through a pluggable protocol.
    public class WebException : InvalidOperationException
    {
        //
        // Summary:
        //     Initializes a new instance of the System.Net.WebException class.
        public WebException() { }
        public WebException(string message): base(message) { }
        //public WebException(string message, WebExceptionStatus status);
        public WebException(string message, Exception innerException) : base(message, innerException) { }
        //public WebException(string message, Exception innerException, WebExceptionStatus status, WebResponse response);
        public WebResponse Response { get; }
        public WebExceptionStatus Status { get; }
    }
}