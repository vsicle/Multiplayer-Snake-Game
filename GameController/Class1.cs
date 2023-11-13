namespace Controller;
using NetworkUtil;

using System.Diagnostics;

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
        }

        /// <summary>
        /// Method to be invoked by the networking library when a connection is made
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
        /*
            if (state.ErrorOccurred)
            {
                // inform the view
                Error?.Invoke("Error connecting to server");
                return;
            }

            // inform the view. Connected? is event type.
            Connected?.Invoke();

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
        // Do event when Server sends player ID and size of world
        Connected?.Invoke();
        Debug.WriteLine("Connected");

        
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
            return;
        }
        ProcessMessages(state);

        // Continue the event loop
        // state.OnNetworkAction has not been changed, 
        // so this same method (ReceiveMessage) 
        // will be invoked when more data arrives
        Networking.GetData(state);
    }


}




