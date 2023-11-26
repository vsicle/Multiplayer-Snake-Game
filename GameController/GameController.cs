namespace Controller;
using Model;
using NetworkUtil;
using System.Text.RegularExpressions;
/// <summary>
/// This class is used to manage communication between the Client
/// and Server.
/// </summary>
public class GameController
{
    // Event for the View to to initialize the World.
    public delegate void InitialMessageHandler(IEnumerable<string> messages, World world);
    public event InitialMessageHandler? InitialMessagesArrived;

    // Event for the View to draw World as Server sends updates.
    public delegate void MessageHandler(IEnumerable<string> messages);
    public event MessageHandler? MessagesArrived;

    public delegate void ErrorHandler(string err);
    public event ErrorHandler? Error;

    private string? PlayerName;
    private bool handshakeComplete;
    public int playerID;
    private World world;
    private string? movementRequest;

    private SocketState? sendSocket;

    public GameController()
    {
        // Initialized but not used, will be replaced once connection is established
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

        if (state.ErrorOccurred)
        {
            // inform the view
            Error?.Invoke("Error connecting to server");

            return;
        }

        sendSocket = state;
        // Initiate protocol with Server.
        Networking.Send(state.TheSocket, PlayerName + "\n");

        state.OnNetworkAction = ReceiveMessage;
        Networking.GetData(state);

    }
    /// <summary>
    /// Changes controller's movement request variable based on 
    /// View's passed input.
    /// </summary>
    /// <param name="request">View's movement request passed to Controller.</param>

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
    /// data is available from Server.
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

        // deal with movement request
        if (movementRequest != null && sendSocket != null)
        {
            lock (sendSocket)
            {
                // send the request
                Networking.Send(state.TheSocket, movementRequest + "\n");
                // set it to null to signify request has been processed
                movementRequest = null;
            }
        }


        // Parse messages differently, depending on 
        // stage of protocol.
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
    /// <summary>
    /// Method to process powerups, snakes,
    /// and any other event data from Server.
    /// </summary>
    /// <param name="state"></param>
    private void ProcessMessages(SocketState state)
    {
        // Assemble incoming data from server
        List<string> incomingData = BuildIncomingData(state);

        lock (world)
        {
            // update model using the data
            for (int i = 0; i < incomingData.Count; i++)
            {
                if (incomingData[i].Length != 0)
                    world.UpdateWorld(incomingData[i]);
            }
        }

        // Tell View that the world has changed
        // so it can draw the world.
        MessagesArrived?.Invoke(incomingData);
    }

    /// <summary>
    /// Method to assemble the incoming data into a single, ready to use list
    /// Process any buffered messages separated by '\n'
    /// </summary>
    /// <param name="state"></param>
    /// <returns>List<string> Representing the incoming data in a string list</string></returns>
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
    /// Pocesses the handshake part of the incoming messages from the server
    /// Then inform the view.
    /// </summary>
    /// <param name="state"></param>
    private void ProcessInitialMessages(SocketState state)
    {

        // build the incoming messages
        List<string> parts = BuildIncomingData(state);

        // capture player ID
        playerID = int.Parse(parts[0]);

        // give the world the worldSize (in pixels)
        // create the actual world class we will use
        world = new World(int.Parse(parts[1]));

        // Update walls in Model.
        lock (world)
        {
            for (int i = 2; i < parts.Count; i++)
            {
                world.UpdateWorld(parts[i]);
            }
            // set flag for completed handshake
            handshakeComplete = true;
        }

        // inform the view of the info about the handshake and give it the world so it has access
        InitialMessagesArrived?.Invoke(parts, world);
    }
}




