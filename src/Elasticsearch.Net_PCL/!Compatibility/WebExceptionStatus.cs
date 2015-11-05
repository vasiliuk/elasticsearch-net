namespace System.Net
{
    //
    // Summary:
    //     Defines status codes for the System.Net.WebException class.
    public enum WebExceptionStatus
    {
        //
        // Summary:
        //     No error was encountered.
        Success = 0,
        //
        // Summary:
        //     The name resolver service could not resolve the host name.
        NameResolutionFailure = 1,
        //
        // Summary:
        //     The remote service point could not be contacted at the transport level.
        ConnectFailure = 2,
        //
        // Summary:
        //     A complete response was not received from the remote server.
        ReceiveFailure = 3,
        //
        // Summary:
        //     A complete request could not be sent to the remote server.
        SendFailure = 4,
        //
        // Summary:
        //     The request was a piplined request and the connection was closed before the response
        //     was received.
        PipelineFailure = 5,
        //
        // Summary:
        //     The request was canceled, the System.Net.WebRequest.Abort method was called,
        //     or an unclassifiable error occurred. This is the default value for System.Net.WebException.Status.
        RequestCanceled = 6,
        //
        // Summary:
        //     The response received from the server was complete but indicated a protocol-level
        //     error. For example, an HTTP protocol error such as 401 Access Denied would use
        //     this status.
        ProtocolError = 7,
        //
        // Summary:
        //     The connection was prematurely closed.
        ConnectionClosed = 8,
        //
        // Summary:
        //     A server certificate could not be validated.
        TrustFailure = 9,
        //
        // Summary:
        //     An error occurred while establishing a connection using SSL.
        SecureChannelFailure = 10,
        //
        // Summary:
        //     The server response was not a valid HTTP response.
        ServerProtocolViolation = 11,
        //
        // Summary:
        //     The connection for a request that specifies the Keep-alive header was closed
        //     unexpectedly.
        KeepAliveFailure = 12,
        //
        // Summary:
        //     An internal asynchronous request is pending.
        Pending = 13,
        //
        // Summary:
        //     No response was received during the time-out period for a request.
        Timeout = 14,
        //
        // Summary:
        //     The name resolver service could not resolve the proxy host name.
        ProxyNameResolutionFailure = 15,
        //
        // Summary:
        //     An exception of unknown type has occurred.
        UnknownError = 16,
        //
        // Summary:
        //     A message was received that exceeded the specified limit when sending a request
        //     or receiving a response from the server.
        MessageLengthLimitExceeded = 17,
        //
        // Summary:
        //     The specified cache entry was not found.
        CacheEntryNotFound = 18,
        //
        // Summary:
        //     The request was not permitted by the cache policy. In general, this occurs when
        //     a request is not cacheable and the effective policy prohibits sending the request
        //     to the server. You might receive this status if a request method implies the
        //     presence of a request body, a request method requires direct interaction with
        //     the server, or a request contains a conditional header.
        RequestProhibitedByCachePolicy = 19,
        //
        // Summary:
        //     This request was not permitted by the proxy.
        RequestProhibitedByProxy = 20
    }
}