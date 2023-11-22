# game-giterdone_game By Eric Nee & Vasko VASSILEV
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
ask T.A’s tomorrow morning. Set up GameController, Model projects. Switched PS7 Network Controller 
to provided Network Controller DLL. PS8 solution is called SnakeGame.

11/13/2023 – Posted on piazza about wether we can switch out the toCall method to handle
different phases of the code, or if everything needs to go into one method with Boolean 
switches. Started a model class. Got all messages from public server to print out, SUCCESSFUL CONNECTION.
T.A. said to make everything in one toCall method with boolean flags and
that we would talk about how to swap out toCall methods tomorrow in lecture.

Added code to capture player ID and world size from server (first two ints recieved)

11/14/2023 - Began World class in Model project. Added a method to WordClass called UpdateWorld that GameController 
calls to update World. Added JSONInclude tags to Snake game objects & their fields. GameController includes two callbacks for each
Client. First callback (OnConnect) handles connecting to server, and the second (ReceiveMessage) handles getting data from server.
ReceiveMessages contains flags to invoke different message processing methods, depending on what data Server sends.

11/16/2023 - Resolved JSONDocument parsing root element of object type issues. Began drawing in WorldPanel.

11/17/2023 - Fixed issue with updating Walls in World model. Vasko began drawing Wall logic. At this stage,
we successfully drew background and translated view on player's snake head.

11/20/2023 - Vasko worked on drawing Wall and Snake segments.

11/21/2023 - Fixed movement request issue. Added locks around places where World was being updated & drawn. Wall & Snake 
segment drawing commpleted. Added logic to remove dead powerups & disconnected snakes from World. 3 Clients playing @ 30 fps 
works.


