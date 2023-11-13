# game-giterdone_game By Eric Nee & VASIL VASSILEV
game-giterdone_game created by GitHub Classroom
PS7
  11/3
    - Implemented StartServer, ConnectToServer methods & their callbacks
  11/5
    - Implemented GetData, Send & their callbacks
  11/6
    - Implemented Send&Close method & its callback
    - First simple tests with FCS seem to work
    - For timeout tests:
      - Added Thread.Sleep(10000) to multiple places, including inside ConnectToServer/callback, ReceiveCallback, Server.AcceptNewClient,
        Client.OnConnectClicked & OnConnect callback, but no Error message prompted in client.

PS8

11/12/2023 – Started the project, got the handshake to work. Now printing connected once we
are connected to the public server, can’t get incoming state information to print, going to
ask T.A’s tomorrow morning

11/13/2023 – Posted on piazza about wether we can switch out the toCall method to handle
different phases of the code, or if everything needs to go into one method with Boolean 
switches. Started a model class. Got all messages from public server to print out, SUCCESSFUL CONNECTION.
T.A. said to make everything in one toCall method with boolean flags and
that we would talk about how to swap out toCall methods tomorrow in lecture.

Added code to capture player ID and world size from server (first two ints recieved)

