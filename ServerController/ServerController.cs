using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using NetworkUtil;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Text.Json;
using Model;
using SnakeGame;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Xml.Linq;

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

        

        //private string TempPlayerName;

        public ServerController(ServerWorld _world)
        {
            IdMap = new Dictionary<long, int>();
            Clients = new List<SocketState>();
            world = _world;
            world.snakes = new Dictionary<int, Snake>();
            world.powerups = new Dictionary<int, Powerup>();

            // fill cardinalDirections dictionary
            // TODO: make readonly if time allows
            cardinalDirections = new Dictionary<string, Vector2D>();
            cardinalDirections["up"] = new Vector2D(0, -1);
            cardinalDirections["down"] = new Vector2D(0, 1);
            cardinalDirections["right"] = new Vector2D(1, 0);
            cardinalDirections["left"] = new Vector2D(-1, 0);
            ClientMoveRequests = new List<Tuple<long, String>>();
        }

        static void Main(string[] args)
        {
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

            if (XMLWorld != null)
            {
                while (XMLWorld.MaxPowerups > 100 || XMLWorld.SnakeSpeed > 9 || XMLWorld.StartingSnakeLength > 360 || XMLWorld.SnakeGrowth > 600)
                {
                    Console.WriteLine("A default value in the XML settings is too large. Decrease MaxPowerups, DefaultSnakeSpeed or " +
                        "StartingSnakeLength, close, and restart Server");
                }
                ServerController server = new ServerController(XMLWorld);
                server.StartServer();

                Stopwatch sw = new Stopwatch();

                int FPSCounter = 0;

                while (true)
                {
                    
                    sw.Start();

                    while (sw.ElapsedMilliseconds < (long)server.world.MSPerFrame) { }

                    sw.Restart();

                    FPSCounter++;

                    if (FPSCounter >= 3)
                    {
                        FPSCounter = 0;
                        Console.WriteLine("FPS: " + server.world.MSPerFrame);
                    }

                    lock(server.ClientMoveRequests)
                    {
                        
                        foreach (Tuple<long, String> ClientRequest in server.ClientMoveRequests)
                        {
                            server.ProcessMovementRequest(ClientRequest.Item2, ClientRequest.Item1);
                            
                        }
                        // Clear ClientRequests
                        
                        server.ClientMoveRequests.Clear();

                    }
                    

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

            Console.WriteLine("Client connected, ID: "+state.ID);

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

            List<string> data = BuildIncomingData(state);

            string newPlayerName = data[0];

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (Clients)
            {
                numClients++;
                Clients.Add(state);
                IdMap[state.ID] = numClients;
            }

            // TODO: Send PlayerID, but wasn't sure 
            // where we should save PlayerID in Server

            // save player ID in Dictionary?

            

            lock(world) {

                Networking.Send(state.TheSocket, numClients + "\n" + world.UniverseSize.ToString() + "\n");
               

                List<Vector2D> TempBody = new List<Vector2D>();
                TempBody.Add(new Vector2D(0, -world.StartingSnakeLength));
                TempBody.Add(new Vector2D(0, 0));

                Snake NewSnake = new Snake(numClients, newPlayerName, TempBody, new Vector2D(0, -1), 0, false, true, false, true);
                world.snakes.Add(numClients, NewSnake);
            }
            

            lock (world.Walls)
            {
                foreach (Wall wall in world.Walls)
                {
                    Networking.Send(state.TheSocket, JsonSerializer.Serialize(wall) + "\n");
                }
                
            }

            // TODO:
            // change the state's network action to the 
            // receive handler so we can process data when something
            // happens on the network
            //state.OnNetworkAction = ReceiveMessage;
            state.OnNetworkAction = NormalOp;
            Networking.GetData(state);
        }

        private void NormalOp(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                lock (world.snakes)
                {
                    world.snakes.Remove(IdMap[state.ID]);
                }
                Console.WriteLine("Client " +  state.ID + " disconnected");
                return;
            }
                

            List<string> movementRequests = BuildIncomingData(state);

            if(movementRequests.Count > 0)
            {

                // Add request to List
                // to prevent trying to modify Snake.dir and 
                // Serializing snakes
                lock (ClientMoveRequests)
                {
                    ClientMoveRequests.Add(new Tuple<long, String>(state.ID, movementRequests[0]));
                }
            }

            Networking.GetData(state);

        }

        private void ProcessMovementRequest(string request, long stateId)
        {
            lock (world)
            {
                int snakeId = IdMap[stateId];

                Snake snake = world.snakes[snakeId];

                CtrlCommand? command = JsonSerializer.Deserialize<CtrlCommand>(request);
                if (command == null)
                {
                    return;
                }

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
                    //snake.dir = moveRequest;
                    //Debug.WriteLine("Changed snake dir to: "+ moveRequest.ToString());
                    //Console.WriteLine("Changed snake dir to: "+ moveRequest.ToString());
                    snake.ChangeSnakeDirection(moveRequest, world.SnakeSpeed);

                }
            }
            
        }



        private void UpdateWorld()
        {
            
            lock (Clients)
            {
                lock (world)
                {
                    foreach (Snake snake in world.snakes.Values)
                    {
                        snake.MoveSnake(world.SnakeSpeed);

                    }
                }

                lock (world.powerups)
                {
                    foreach (Powerup powerup in world.powerups.Values)
                    {
                        if (powerup.died)
                        {
                            world.powerups.Remove(powerup.power);
                        }
                    }

                    if (world.powerups.Count < world.MaxPowerups)
                    {

                        if (!WaitingPowerUp)
                        {
                            Random rand = new Random();
                            RandPowerUpDelay = rand.Next(0, world.PowerupDelay);

                            PowerUpCounter = 0;

                            WaitingPowerUp = true;

                        }
                        else if (PowerUpCounter >= world.PowerupDelay)
                        {
                            Random rand = new Random();
                            double XCord = (double)rand.Next(-((world.UniverseSize - world.StartingSnakeLength) / 2), ((world.UniverseSize - world.StartingSnakeLength) / 2));
                            double YCord = (double)rand.Next(-((world.UniverseSize - world.StartingSnakeLength) / 2), ((world.UniverseSize - world.StartingSnakeLength) / 2));

                            Powerup tempPowerup = new Powerup(NumPowerUps, new Vector2D(XCord, YCord), false);
                            bool WallCollision = false;

                            foreach (Wall wall in world.Walls)
                            {
                                if (tempPowerup.RectangleCollision(wall.p1, wall.p2, 25))
                                {
                                    WallCollision = true;
                                }
                            }

                            if (!WallCollision)
                            {
                                world.powerups.Add(NumPowerUps, tempPowerup);
                                NumPowerUps++;
                                WaitingPowerUp = false;
                            }

                        }
                        else
                        {
                            PowerUpCounter++;
                        }

                    }
                }


                foreach (SocketState state in Clients)
                {
                    lock (world.snakes) 
                    {
                        // send all objects in the current world,
                        // TODO: maybe copy or move this somewhere?
                        foreach (Snake snake in world.snakes.Values)
                        {
                            if (snake.IsGrowing && snake.SnakeGrowCounter < world.SnakeGrowth)
                            {
                                snake.SnakeGrowCounter++;
                            } else
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

                            foreach (Powerup powerup in world.powerups.Values)
                            {
                                if(snake.PowerUpCollision(powerup.loc))
                                {
                                    powerup.died = true;
                                    snake.IsGrowing = true;
                                    
                                }
                            }

                            // Find first segment of snake to check against collisions
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
                                    int RandCoord = rand.Next(-((world.UniverseSize - world.StartingSnakeLength) / 2), ((world.UniverseSize - world.StartingSnakeLength) / 2));
                                    double coordinate = (double)RandCoord;
                                    snake.alive = true;
                                    snake.respawnCounter = 0;
                                    snake.dir = cardinalDirections["down"];
                                    snake.body = new List<Vector2D>();
                                    // make tail of snake
                                    snake.body.Add(new Vector2D(coordinate, coordinate - world.StartingSnakeLength));
                                    // make head of snake
                                    snake.body.Add(new Vector2D(coordinate, coordinate));
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

                                
                                // snake.MoveSnake(somethings to add)
                                Networking.Send(state.TheSocket, JsonSerializer.Serialize(snake) + "\n");
                            }
                        }
                    }
                    
                    

                    foreach (Powerup powerup in world.powerups.Values)
                    {
                        Networking.Send(state.TheSocket, JsonSerializer.Serialize(powerup) + "\n");

                    }

                }
                
            }

        }
    }
}