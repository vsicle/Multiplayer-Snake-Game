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
