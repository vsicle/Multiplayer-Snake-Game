using Model;
using NetworkUtil;
using SnakeGame;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace ServerController
{
    public class ServerController
    {
        // Map of Clients
        private Dictionary<long, int> IdMap;
        private List<SocketState> Clients;
        private Dictionary<string, Vector2D> cardinalDirections;

        private ServerWorld world;
        private int numClients;
        private List<Tuple<long, String>> ClientMoveRequests;

        // Counters for spawning powerups
        private int PowerUpCounter;
        private int RandPowerUpDelay;
        private bool WaitingPowerUp;
        private int NumPowerUps;

        /// <summary>
        /// constructor for a server
        /// </summary>
        /// <param name="_world"></param>
        public ServerController(ServerWorld _world)
        {
            IdMap = new Dictionary<long, int>();
            Clients = new List<SocketState>();
            world = _world;
            world.snakes = new Dictionary<int, Snake>();
            world.powerups = new Dictionary<int, Powerup>();

            // fill cardinalDirections dictionary
            cardinalDirections = new Dictionary<string, Vector2D>();
            cardinalDirections["up"] = new Vector2D(0, -1);
            cardinalDirections["down"] = new Vector2D(0, 1);
            cardinalDirections["right"] = new Vector2D(1, 0);
            cardinalDirections["left"] = new Vector2D(-1, 0);
            ClientMoveRequests = new List<Tuple<long, String>>();
        }

        /// <summary>
        /// main method for the server, maintains ticks for framerate
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // read in all XML settings
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            ServerWorld? XMLWorld;
            using (XmlReader reader = XmlReader.Create("WorldSettings.xml", settings))
            {

                DataContractSerializer serializer = new DataContractSerializer(typeof(ServerWorld));
                XMLWorld = (ServerWorld?)serializer.ReadObject(reader);
                while (XMLWorld == null)
                {
                    Console.WriteLine("World is null, possibly incorrect world settings. Fix XML document, close and restart Server.");
                }

            }

            // if the settings were read correctly
            if (XMLWorld != null)
            {
                while (XMLWorld.MaxPowerups > 100 || XMLWorld.SnakeSpeed > 9 || XMLWorld.StartingSnakeLength > 360 || XMLWorld.SnakeGrowth > 600)
                {
                    Console.WriteLine("A default value in the XML settings is too large. Decrease MaxPowerups, DefaultSnakeSpeed or " +
                        "StartingSnakeLength, close, and restart Server");
                }
                ServerController server = new ServerController(XMLWorld);
                server.StartServer();

                // stopwatch for frame rate
                Stopwatch sw = new Stopwatch();

                // takes care of printing the framerate
                int FPSCounter = 0;
                int PrintCounter = 1000 / server.world.MSPerFrame;

                sw.Start();

                // infinite loop to send out updates
                while (true)
                {

                    while (sw.ElapsedMilliseconds < (long)server.world.MSPerFrame) { }

                    sw.Restart();

                    FPSCounter++;

                    if (FPSCounter >= PrintCounter)
                    {
                        Console.WriteLine("FPS: " + PrintCounter);
                        FPSCounter = 0;
                    }

                    // process movement requests
                    lock (server.ClientMoveRequests)
                    {

                        foreach (Tuple<long, String> ClientRequest in server.ClientMoveRequests)
                        {
                            server.ProcessMovementRequest(ClientRequest.Item2, ClientRequest.Item1);

                        }
                        // Clear ClientRequests

                        server.ClientMoveRequests.Clear();

                    }

                    // call method that updates everything in the world
                    server.UpdateWorld();

                }
            }



        }

        /// <summary>
        /// Start accepting Tcp sockets connections from clients
        /// </summary>
        private void StartServer()
        {
            // This begins an "event loop"
            Networking.StartServer(NewClientConnected, 11000);
            numClients = 0;
            Console.WriteLine("Accepting new clients");


        }

        /// <summary>
        /// Method to be invoked by the networking library
        /// when a new client connects
        /// </summary>
        /// <param name="state">The SocketState representing the new client</param>
        private void NewClientConnected(SocketState state)
        {
            if (state.ErrorOccurred)
                return;

            Console.WriteLine("Client connected, ID: " + state.ID);

            state.OnNetworkAction = ConnectedCallBack;
            Networking.GetData(state);

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

                // build the list of messages
                newMessages.Add(p);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            return newMessages;
        }

        /// <summary>
        /// Handle recieving the player name upon initial connection
        /// </summary>
        /// <param name="state"></param>
        private void ConnectedCallBack(SocketState state)
        {
            // assemble the data coming in
            List<string> data = BuildIncomingData(state);

            // save the playerName
            string newPlayerName = data[0];

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (Clients)
            {
                numClients++;
                Clients.Add(state);
                IdMap[state.ID] = numClients;
            }


            lock (world)
            {

                // send client ID and universe size
                Networking.Send(state.TheSocket, numClients + "\n" + world.UniverseSize.ToString() + "\n");

                // create body of snake
                List<Vector2D> TempBody = new List<Vector2D>();
                TempBody.Add(new Vector2D(0, -world.StartingSnakeLength));
                TempBody.Add(new Vector2D(0, 0));

                // create snake
                Snake NewSnake = new Snake(numClients, newPlayerName, TempBody, new Vector2D(0, -1), 0, false, true, false, true);

                // add the snake to the world
                world.snakes.Add(numClients, NewSnake);
            }


            lock (world.Walls)
            {
                // send every wall, lock is slightly overkill but keeps everything safe
                foreach (Wall wall in world.Walls)
                {
                    Networking.Send(state.TheSocket, JsonSerializer.Serialize(wall) + "\n");
                }

            }

            // switch the toCall method to the normal operation method that handles movement requests
            state.OnNetworkAction = NormalOp;
            Networking.GetData(state);
        }

        /// <summary>
        /// metyhod to handle incomign messages (movement requests)
        /// </summary>
        /// <param name="state"></param>
        private void NormalOp(SocketState state)
        {
            // check for disconnected clients
            if (state.ErrorOccurred)
            {
                lock (world.snakes)
                {
                    // set the snake to disconnected, so that can be handled later
                    Snake? DCSnake = world.snakes.GetValueOrDefault(IdMap[state.ID]);
                    if (DCSnake != null)
                    {
                        DCSnake.dc = true;
                    }

                }
                Console.WriteLine("Client " + state.ID + " disconnected");
                return;
            }

            // assemble the incoming message
            List<string> movementRequests = BuildIncomingData(state);

            // if theres more than zero movement requests add them to the list of movement requests
            if (movementRequests.Count > 0)
            {
                // Add request to List
                // to prevent trying to modify Snake.dir and 
                // Serializing snakes
                lock (ClientMoveRequests)
                {
                    ClientMoveRequests.Add(new Tuple<long, String>(state.ID, movementRequests[0]));
                }
            }

            // start recieving data
            Networking.GetData(state);

        }

        /// <summary>
        /// deal with movement requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="stateId"></param>
        private void ProcessMovementRequest(string request, long stateId)
        {
            lock (world)
            {
                // look up the snake in the map of ID's
                int snakeId = IdMap[stateId];

                Snake snake = world.snakes[snakeId];

                // deserialize the command
                CtrlCommand? command = JsonSerializer.Deserialize<CtrlCommand>(request);
                if (command == null)
                {
                    return;
                }

                // see if its a valid command
                if (!cardinalDirections.ContainsKey(command.moving))
                {
                    return;
                }
                Vector2D moveRequest = cardinalDirections[command.moving];

                // If command is same as snake's current direction, 
                // ignore request.
                if (moveRequest.Equals(snake.dir)) { return; }

                // Find out how long Snake head segment is.
                Vector2D HeadSegment = snake.body[snake.body.Count - 1] - snake.body[snake.body.Count - 2];

                // If movement request is not opposite cardinal direction & head segment is sufficiently long
                // enough (to avoid snake 180 self collisions), change snake direction.
                if (!snake.dir.IsOppositeCardinalDirection(moveRequest) && HeadSegment.Length() > 10.0)
                {
                    snake.ChangeSnakeDirection(moveRequest, world.SnakeSpeed);

                }
            }

        }


        /// <summary>
        /// The method that updates the world, checks for collisions, powerups, disconnected snakes, etc.
        /// </summary>
        private void UpdateWorld()
        {

            lock (Clients)
            {
                lock (world)
                {
                    foreach (Snake snake in world.snakes.Values)
                    {
                        // check all snakes for disconnection
                        if (snake.dc)
                        {
                            lock (Clients)
                            {
                                foreach (SocketState Client in Clients)
                                {
                                    // send the disconnected snake
                                    Networking.Send(Client.TheSocket, JsonSerializer.Serialize(snake) + "\n");
                                }
                            }
                            // remove the disconnected snake
                            world.snakes.Remove(snake.snake);
                        }

                        // if snake is not disconnected move it in the direction its facing
                        snake.MoveSnake(world.SnakeSpeed);

                    }
                }


                lock (world.powerups)
                {
                    foreach (Powerup powerup in world.powerups.Values)
                    {
                        // see if powerup has been consumed
                        if (powerup.died)
                        {
                            // remove the powerup from the list if its been consumed
                            world.powerups.Remove(powerup.power);
                        }
                    }

                    // if theres less powerups than the max then get a delay and generate them accordingly
                    if (world.powerups.Count < world.MaxPowerups)
                    {
                        // if its time to start a waiting period start it
                        if (!WaitingPowerUp)
                        {
                            Random rand = new Random();
                            RandPowerUpDelay = rand.Next(0, world.PowerupDelay);

                            PowerUpCounter = 0;

                            WaitingPowerUp = true;

                        }
                        // if the delay is up, spawn a powerup in a random location
                        else if (PowerUpCounter >= world.PowerupDelay)
                        {
                            Random rand = new Random();
                            double XCord = (double)rand.Next(-((world.UniverseSize - world.StartingSnakeLength) / 2), ((world.UniverseSize - world.StartingSnakeLength) / 2));
                            double YCord = (double)rand.Next(-((world.UniverseSize - world.StartingSnakeLength) / 2), ((world.UniverseSize - world.StartingSnakeLength) / 2));

                            Powerup tempPowerup = new Powerup(NumPowerUps, new Vector2D(XCord, YCord), false);
                            bool WallCollision = false;

                            // see if the location conflicts with a wall
                            foreach (Wall wall in world.Walls)
                            {
                                if (tempPowerup.RectangleCollision(wall.p1, wall.p2, 35))
                                {
                                    WallCollision = true;
                                }
                            }

                            // if it deosnt conflict with a wall spawn it
                            if (!WallCollision)
                            {
                                world.powerups.Add(NumPowerUps, tempPowerup);
                                NumPowerUps++;
                                WaitingPowerUp = false;
                            }

                        }
                        else
                        {
                            // waiting for timer, increment it
                            PowerUpCounter++;
                        }

                    }
                }

                // for every client
                foreach (SocketState state in Clients)
                {
                    lock (world.snakes)
                    {
                        // send all objects in the current world
                        foreach (Snake snake in world.snakes.Values)
                        {
                            // grow snake if needed
                            if (snake.IsGrowing && snake.SnakeGrowCounter < world.SnakeGrowth)
                            {
                                snake.SnakeGrowCounter++;
                            }
                            else
                            {
                                snake.SnakeGrowCounter = 0;
                                snake.IsGrowing = false;
                            }
                            Vector2D HeadDir = snake.dir;
                            // check for collisions, kill snake if needed
                            foreach (Wall wall in world.Walls)
                            {
                                if (snake.RectangleCollision(wall.p1, wall.p2, 25))
                                {
                                    // if snake collides with wall, kill it
                                    snake.alive = false;
                                }
                            }

                            // send out powerups
                            foreach (Powerup powerup in world.powerups.Values)
                            {
                                if (snake.PowerUpCollision(powerup.loc))
                                {
                                    powerup.died = true;
                                    snake.IsGrowing = true;

                                }
                            }

                            // Collision checking snake to self
                            int StartVertexCollisionCheck = 1;

                            for (int i = snake.body.Count - 3; i >= 1; i--)
                            {

                                Vector2D FirstSegment = snake.body[i];
                                Vector2D SecondSegment = snake.body[i - 1];

                                Vector2D SegmentDir = FirstSegment - SecondSegment;
                                SegmentDir.Normalize();

                                if (HeadDir.IsOppositeCardinalDirection(SegmentDir))
                                {
                                    StartVertexCollisionCheck = i;
                                    break;
                                }
                            }

                            // check head to body collisions for a snake to itself
                            if (StartVertexCollisionCheck >= 1 && snake.body.Count > 3)
                            {
                                for (int i = StartVertexCollisionCheck; i > 0; i--)
                                {
                                    Vector2D FirstSegment = snake.body[i];
                                    Vector2D SecondSegment = snake.body[i - 1];

                                    Vector2D SegmentDir = FirstSegment - SecondSegment;
                                    SegmentDir.Normalize();

                                    if (snake.RectangleCollision(FirstSegment, SecondSegment, 5))
                                    {
                                        snake.alive = false; break;
                                    }

                                }
                            }

                            // collision checking head to other snake segments
                            foreach (Snake OtherSnakes in world.snakes.Values)
                            {
                                if (OtherSnakes.snake != snake.snake)
                                {
                                    for (int i = 1; i < OtherSnakes.body.Count; i++)
                                    {

                                        List<Vector2D> segment = OtherSnakes.body.GetRange(i - 1, 2);
                                        if (snake.RectangleCollision(segment[0], segment[1], 5.0))
                                        {
                                            snake.alive = false;
                                        }
                                    }
                                }
                            }


                            // if snake is dead, respwan
                            if (!snake.alive)
                            {
                                if (snake.respawnCounter >= world.RespawnRate)
                                {
                                    Random rand = new Random();
                                    int RandCord1 = rand.Next(-((world.UniverseSize - world.StartingSnakeLength) / 2), ((world.UniverseSize - world.StartingSnakeLength) / 2));
                                    int RandCord2 = rand.Next(-((world.UniverseSize - world.StartingSnakeLength) / 2), ((world.UniverseSize - world.StartingSnakeLength) / 2));
                                    double coordinateX = (double)RandCord1;
                                    double coordinateY = (double)RandCord2;

                                    snake.alive = true;
                                    snake.respawnCounter = 0;
                                    snake.dir = cardinalDirections["down"];
                                    snake.body = new List<Vector2D>();
                                    // make tail of snake
                                    snake.body.Add(new Vector2D(coordinateX, coordinateY - world.StartingSnakeLength));
                                    // make head of snake
                                    snake.body.Add(new Vector2D(coordinateX, coordinateY));
                                }
                                else
                                {
                                    // increment respawn counter
                                    snake.respawnCounter++;
                                }
                            }
                            else
                            {
                                // snake is alive, move it and send it
                                Networking.Send(state.TheSocket, JsonSerializer.Serialize(snake) + "\n");
                            }
                        }
                    }

                    // send all powerups
                    foreach (Powerup powerup in world.powerups.Values)
                    {
                        Networking.Send(state.TheSocket, JsonSerializer.Serialize(powerup) + "\n");
                    }
                }
            }
        }
    }
}