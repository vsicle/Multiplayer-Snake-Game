﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkUtil;

public static class Networking
{
    /////////////////////////////////////////////////////////////////////////////////////////
    // Server-Side Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
    /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
    /// AcceptNewClient will continue the event-loop.
    /// </summary>
    /// <param name="toCall">The method to call when a new connection is made</param>
    /// <param name="port">The the port to listen on</param>
    public static TcpListener StartServer(Action<SocketState> toCall, int port)
    {
        // create a listener for the server
        TcpListener server = new TcpListener(IPAddress.Any, port);

        // put the listener and the "done" method in a tuple to pass them along
        Tuple<TcpListener, Action<SocketState>> storage = new Tuple<TcpListener, Action<SocketState>>(server, toCall);

        // start the listener
        
        server.Start();

        // start accepting
        server.BeginAcceptSocket(AcceptNewClient, storage);

        return server;
    }

    /// <summary>
    /// To be used as the callback for accepting a new client that was initiated by StartServer, and 
    /// continues an event-loop to accept additional clients.
    ///
    /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
    /// OnNetworkAction should be set to the delegate that was passed to StartServer.
    /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action. 
    /// 
    /// If anything goes wrong during the connection process (such as the server being stopped externally), 
    /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true 
    /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
    /// an error occurs.
    ///
    /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept 
    /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
    /// </summary>
    /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with 
    /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
    private static void AcceptNewClient(IAsyncResult ar)
    {

        // save the tuple
        Tuple<TcpListener, Action<SocketState>> incoming = (Tuple<TcpListener, Action<SocketState>>)ar.AsyncState!;

        // unpack the tuple
        TcpListener server = incoming.Item1;
        Action<SocketState> toCall = incoming.Item2;


        try
        {
            // "confirm" the connection
            Socket socket = server.EndAcceptSocket(ar);

            // create a socket state for the new connection
            SocketState state = new SocketState(toCall, socket);

            // successful connection, invoke the delegate
            state.OnNetworkAction(state);
        }
        catch (Exception e)
        {
            // error, create blank socket 
            SocketState error = new SocketState(toCall, e.Message);
            error.ErrorOccurred = true;
            toCall(error);
            return;
        }


        server.BeginAcceptSocket(AcceptNewClient, new Tuple<TcpListener, Action<SocketState>>(server, toCall));
    }

    /// <summary>
    /// Stops the given TcpListener.
    /// </summary>
    public static void StopServer(TcpListener listener)
    {
        // stop the listener, stops the server
        listener.Stop();
    }

    /////////////////////////////////////////////////////////////////////////////////////////
    // Client-Side Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Begins the asynchronous process of connecting to a server via BeginConnect, 
    /// and using ConnectedCallback as the method to finalize the connection once it's made.
    /// 
    /// If anything goes wrong during the connection process, toCall should be invoked 
    /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message 
    /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
    /// in this method or in ConnectedCallback.
    ///
    /// This connection process should timeout and produce an error (as discussed above) 
    /// if a connection can't be established within 3 seconds of starting BeginConnect.
    /// 
    /// </summary>
    /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
    /// <param name="hostName">The server to connect to</param>
    /// <param name="port">The port on which the server is listening</param>
    public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
    {


        // Establish the remote endpoint for the socket.
        IPHostEntry ipHostInfo;
        IPAddress ipAddress = IPAddress.None;

        // Determine if the server address is a URL or an IP
        try
        {
            ipHostInfo = Dns.GetHostEntry(hostName);
            bool foundIPV4 = false;
            foreach (IPAddress addr in ipHostInfo.AddressList)
                if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    foundIPV4 = true;
                    ipAddress = addr;
                    break;
                }
            // Didn't find any IPV4 addresses
            if (!foundIPV4)
            {

                SocketState ErrorSocketState = new SocketState(toCall, "IPV4 address not found");
                ErrorSocketState.ErrorOccurred = true;
                ErrorSocketState.OnNetworkAction.Invoke(ErrorSocketState);

                return;
            }
        }
        catch (Exception)
        {
            // see if host name is a valid ipaddress
            try
            {
                ipAddress = IPAddress.Parse(hostName);
            }
            catch
            {
                // make a dummy socket to set error
                SocketState ErrorSocketState = new SocketState(toCall, "Invalid host name");
                ErrorSocketState.ErrorOccurred = true;
                ErrorSocketState.OnNetworkAction.Invoke(ErrorSocketState);
                return;
            }
        }

        // Create a TCP/IP socket.
        Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // This disables Nagle's algorithm (google if curious!)
        // Nagle's algorithm can cause problems for a latency-sensitive 
        // game like ours will be 
        socket.NoDelay = true;


        SocketState ClientConnect = new SocketState(toCall, socket);


        // 3 second timeout in case connection doesn't happen quickly

        IAsyncResult result = ClientConnect.TheSocket.BeginConnect(ipAddress, port, ConnectedCallback, ClientConnect);

        // starts timer
        bool success = result.AsyncWaitHandle.WaitOne(3000);

        if (!success)
        {
            // socket timed out
            socket.Close();
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
    ///
    /// Uses EndConnect to finalize the connection.
    /// 
    /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
    /// either this method or ConnectToServer should indicate the error appropriately.
    /// 
    /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
    /// with a new SocketState representing the new connection.
    /// 
    /// </summary>
    /// <param name="ar">The object asynchronously passed via BeginConnect</param>
    private static void ConnectedCallback(IAsyncResult ar)
    {
        // retrieve SocketState from AsyncState
        SocketState temp = (SocketState)ar.AsyncState!;

        try
        {
            // try to finalize connection
            temp.TheSocket.EndConnect(ar);

        }
        catch (Exception ex)
        {
            // therer was an error, update socketState accordingly
            temp.ErrorOccurred = true;
            temp.ErrorMessage = ex.Message;

        }
        // Calls delegate once connection has been established.
        temp.OnNetworkAction(temp);
    }


    /////////////////////////////////////////////////////////////////////////////////////////
    // Server and Client Common Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback 
    /// as the callback to finalize the receive and store data once it has arrived.
    /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
    /// 
    /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should 
    /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
    /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
    /// in this method or in ReceiveCallback.
    /// </summary>
    /// <param name="state">The SocketState to begin receiving</param>
    public static void GetData(SocketState state)
    {
        try
        {
            // start recieving
            state.TheSocket.BeginReceive(state.buffer, 0, SocketState.BufferSize, SocketFlags.None, ReceiveCallback, state);
        }
        catch (Exception e)
        {
            // error occurred, set flags
            state.ErrorOccurred = true;
            state.ErrorMessage = e.Message;
            state.OnNetworkAction(state);
            return;
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
    /// 
    /// Uses EndReceive to finalize the receive.
    ///
    /// As stated in the GetData documentation, if an error occurs during the receive process,
    /// either this method or GetData should indicate the error appropriately.
    /// 
    /// If data is successfully received:
    ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
    ///      This must be done in a thread-safe manner with respect to the SocketState methods that access or modify its 
    ///      string builder.
    ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
    /// </summary>
    /// <param name="ar"> 
    /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
    /// </param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
        // data incoming, finish the recieve

        SocketState state = (SocketState)ar.AsyncState!;

        try
        {
            // finish handshake procedure
            int numBytes = state.TheSocket.EndReceive(ar);

            // clean close socket
            if (numBytes == 0)
            {
                state.ErrorOccurred = true;
                state.ErrorMessage = "Clean remote socket shutdown";
            }

            // multithread safe appending
            lock (state)
            {
                state.data.Append(Encoding.UTF8.GetString(state.buffer, 0, numBytes));
            }
        }
        catch (Exception e)
        {
            // catch erros, set SocketState flags accordingly
            state.ErrorOccurred = true;
            state.ErrorMessage = e.Message;
        }

        // Process finished, call the delegate regardless of outcome
        state.OnNetworkAction(state);
    }

    /// <summary>
    /// Begin the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
    /// 
    /// If the socket is closed, does not attempt to send.
    /// 
    /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
    /// </summary>
    /// <param name="socket">The socket on which to send the data</param>
    /// <param name="data">The string to send</param>
    /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
    public static bool Send(Socket socket, string data)
    {
        // encode the data
        byte[] package = Encoding.UTF8.GetBytes(data);

        try
        {
            // start the sending
            socket.BeginSend(package, 0, package.Length, SocketFlags.None, SendCallback, socket);

            // if we get down to here, it means starting the send was successful
            return true;
        }
        catch
        {
            socket.Close();
            return false;
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a send operation that was initiated by Send.
    ///
    /// Uses EndSend to finalize the send.
    /// 
    /// This method must not throw, even if an error occurred during the Send operation.
    /// </summary>
    /// <param name="ar">
    /// This is the Socket (not SocketState) that is stored with the callback when
    /// the initial BeginSend is called.
    /// </param>
    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState!;

        try
        {
            // finish sending operation
            socket.EndSend(ar);
        }
        catch
        {
            // Don't do anything, per spec
            return;
        }
    }


    /// <summary>
    /// Begin the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
    /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
    /// 
    /// If the socket is closed, does not attempt to send.
    /// 
    /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
    /// </summary>
    /// <param name="socket">The socket on which to send the data</param>
    /// <param name="data">The string to send</param>
    /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
    public static bool SendAndClose(Socket socket, string data)
    {
        // encode the string
        byte[] package = Encoding.UTF8.GetBytes(data);

        try
        {
            //start the send
            socket.BeginSend(package, 0, package.Length, SocketFlags.None, SendAndCloseCallback, socket);

            return true;
        }
        catch (Exception)
        {
            socket.Close();
            return false;
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
    ///
    /// Uses EndSend to finalize the send, then closes the socket.
    /// 
    /// This method must not throw, even if an error occurred during the Send operation.
    /// 
    /// This method ensures that the socket is closed before returning.
    /// </summary>
    /// <param name="ar">
    /// This is the Socket (not SocketState) that is stored with the callback when
    /// the initial BeginSend is called.
    /// </param>
    private static void SendAndCloseCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState!;

        try
        {
            // finish the sending
            socket.EndSend(ar);
        }
        catch
        {
        }
        // close socket to follow "send and close" spec
        socket.Close();
    }
}
