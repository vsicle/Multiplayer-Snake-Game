namespace Controller;
using NetworkUtil;
using Model;

using System.Diagnostics;
using System.Text.RegularExpressions;

public class GameController
{
    // Controller events that the view can subscribe to
    public delegate void MessageHandler(IEnumerable<string> messages);
    public event MessageHandler? MessagesArrived;

    // Could also do public event Action<string> MessageArrived;

    public delegate void ConnectedHandler();
    public event ConnectedHandler? Connected;

    public delegate void ErrorHandler(string err);
    public event ErrorHandler? Error;

    private string? PlayerName;
    private bool handshakeComplete;
    private int playerID;
    private int worldSize;


    /// <summary>
    /// State representing the connection with the server
    /// </summary>
    //SocketState? theServer = null;

    public GameController()
    {

    }

    /// <summary>
    /// Begins the process of connecting to the server
    /// </summary>
    /// <param name="addr"></param>
    public void Connect(string addr, string _PlayerName)
    {
        Networking.ConnectToServer(OnConnect, addr, 11000);

        PlayerName = _PlayerName;
        handshakeComplete = false;
    }

    /// <summary>
    /// Method to be invoked by the networking library when a connection is made
    /// </summary>
    /// <param name="state"></param>
    private void OnConnect(SocketState state)
    {
        /*

            theServer = state;

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);
        */

        if (state.ErrorOccurred)
        {
            // inform the view
            Error?.Invoke("Error connecting to server");

            return;
        }

        Networking.Send(state.TheSocket, PlayerName + "\n");

        Connected?.Invoke();
        Debug.WriteLine("Connected (from controller)");
        state.OnNetworkAction = ReceiveMessage;
        Networking.GetData(state);

    }

    /// <summary>
    /// Method to be invoked by the networking library when 
    /// data is available
    /// </summary>
    /// <param name="state"></param>
    private void ReceiveMessage(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            // inform the view. Error? is event type.
            Error?.Invoke("Lost connection to server");
            // P
            return;
        }

        // Do event when Server sends player ID and size of world

        ProcessMessages(state);

        // Continue the event loop
        // state.OnNetworkAction has not been changed, 
        // so this same method (ReceiveMessage) 
        // will be invoked when more data arrives
        Networking.GetData(state);
    }

    /// <summary>
    /// Process any buffered messages separated by '\n'
    /// Then inform the view
    /// </summary>
    /// <param name="state"></param>
    private void ProcessMessages(SocketState state)
    {
        string totalData = state.GetData();
        string[] parts = Regex.Split(totalData, @"(?<=[\n])");

        // Loop until we have processed all messages.
        // We may have received more than one.

        List<string> newMessages = new List<string>();

        foreach (string p in parts)
        {
            // Ignore empty strings added by the regex splitter
            if (p.Length == 0)
                continue;
            // The regex splitter will include the last string even if it doesn't end with a '\n',
            // So we need to ignore it if this happens. 
            if (p[p.Length - 1] != '\n')
                break;

            // build a list of messages to send to the view
            newMessages.Add(p);

            // Then remove it from the SocketState's growable buffer
            state.RemoveData(0, p.Length);
        }

        // TODO: is this right and reliable???
        if (!handshakeComplete)
        {
            playerID = int.Parse(parts[0]);
            worldSize = int.Parse(parts[1]);
            handshakeComplete = true;
        }


        // inform the view
        MessagesArrived?.Invoke(newMessages);
        // equivalent syntax: MessageArrived(newMessages);

    }
}




