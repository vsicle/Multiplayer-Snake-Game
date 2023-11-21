namespace Controller;
using NetworkUtil;
using Model;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

public class GameController
{
    // Controller events that the view can subscribe to
    public delegate void InitialMessageHandler(IEnumerable<string> messages, World world);
    public event InitialMessageHandler? InitialMessagesArrived;

    public delegate void MessageHandler(IEnumerable<string> messages);
    public event MessageHandler? MessagesArrived;

    // Could also do public event Action<string> MessageArrived;

    public delegate void ConnectedHandler();
    public event ConnectedHandler? Connected;

    public delegate void ErrorHandler(string err);
    public event ErrorHandler? Error;

    private string? PlayerName;
    private bool handshakeComplete;
    public int playerID;
    private World world;
    private string? movementRequest;

    private SocketState? sendSocket;

    /// <summary>
    /// State representing the connection with the server
    /// </summary>
    //SocketState? theServer = null;

    public GameController()
    {
        // not used, will be replaced once connection is established
        world = new World();
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
        state.OnNetworkAction = ReceiveMessage;
        Networking.GetData(state);

    }
    // IMPORTANT THIS SHOULD FIX MOVEMENT CONTROLS
    // TODO: send movement control once we've recieved all data, then handle movement controls

    public void MoveRequest(string request)
    {
        switch (request)
        {
            case "w":
                movementRequest = "{\"moving\":\"up\"}";
                break;
            case "s":
                movementRequest = "{\"moving\":\"down\"}";
                break;
            case "a":
                movementRequest = "{\"moving\":\"left\"}";
                break;
            case "d":
                movementRequest = "{\"moving\":\"right\"}";
                break;
        }
        
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

        // TODO: make this work, maybe move it somehwere
        // deal with movement request
        if (movementRequest != null)
        {
            // send the request
            Debug.WriteLine("Sending movement request: " + movementRequest);
            Networking.Send(state.TheSocket, movementRequest);
            // set it to null to signify request has been processed
            movementRequest = null;
        }

        // Do event when Server sends player ID and size of world
        if (!handshakeComplete)
        {
            ProcessInitialMessages(state);
        }
        else
        {
            ProcessMessages(state);
        }




        // Continue the event loop
        // state.OnNetworkAction has not been changed, 
        // so this same method (ReceiveMessage) 
        // will be invoked when more data arrives
        Networking.GetData(state);
    }

    private void ProcessMessages(SocketState state)
    {
        // Assemble incoming data from server
        List<string> incomingData = BuildIncomingData(state);

        // update model using the data
        for (int i = 0; i < incomingData.Count; i++)
        {
            if (incomingData[i].Length != 0)
                world.UpdateWorld(incomingData[i]);
        }

        


        // Tell View that the world has changed
        MessagesArrived?.Invoke(incomingData);
    }

    /// <summary>
    /// Method to assemble the incoming data into a single, ready to use list
    /// Process any buffered messages separated by '\n'
    /// </summary>
    /// <param name="state"></param>
    /// <returns>List<string> representing the incoming data in a string list</string></returns>
    private List<string> BuildIncomingData(SocketState state)
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



        return newMessages;
    }

    /// <summary>
    /// processes the handshake part of the incoming messages from the server
    /// Then inform the view
    /// </summary>
    /// <param name="state"></param>
    private void ProcessInitialMessages(SocketState state)
    {

        // TODO: Deactivate connect button when connection is established
        sendSocket = state;

        // build the incoming messages
        List<string> parts = BuildIncomingData(state);

        // capture player ID
        playerID = int.Parse(parts[0]);

        // give the world the worldSize (in pixels)
        // create the actual world class we will use
        world = new World(int.Parse(parts[1]));

        for (int i = 2; i < parts.Count; i++)
        {
            world.UpdateWorld(parts[i]);
        }
        // set flag for completed handshake
        handshakeComplete = true;

        
        // inform the view of the info about the handshake and give it the world so it has access
        InitialMessagesArrived?.Invoke(parts, world);


    }
}




