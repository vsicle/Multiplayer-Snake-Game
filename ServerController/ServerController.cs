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
            cardinalDirections["right"] = new Vector2D(-1, 0);
            cardinalDirections["left"] = new Vector2D(1, 0);
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

                while (true)
                {
                    sw.Start();

                    while (sw.ElapsedMilliseconds < server.world.MSPerFrame) { }

                    sw.Restart();
                    Console.WriteLine("Frame");



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

            Networking.Send(state.TheSocket, numClients + "\n" + world.UniverseSize.ToString() + "\n");
            //Networking.Send(state.TheSocket, world.UniverseSize.ToString() + "\n");

            List<Vector2D> TempBody = new List<Vector2D>();
            TempBody.Add(new Vector2D(0, -100));
            TempBody.Add(new Vector2D(0, 0));


            Snake NewSnake = new Snake(numClients, newPlayerName, TempBody, new Vector2D(1, 0), 0, false, true, false, true);
            world.snakes.Add(numClients, NewSnake);

            lock (world)
            {
                foreach (Wall wall in world.Walls)
                {
                    Debug.WriteLine(JsonSerializer.Serialize(wall) + "\n");
                    Networking.Send(state.TheSocket, JsonSerializer.Serialize(wall) + "\n");
                }

                // send all objects in the current world,
                // TODO: maybe copy or move this somewhere?
                foreach (Snake snake in world.snakes.Values)
                {
                    Debug.WriteLine(JsonSerializer.Serialize(snake) + "\n");
                    Networking.Send(state.TheSocket, JsonSerializer.Serialize(snake) + "\n");
                }

                foreach (Powerup powerup in world.powerups.Values)
                {
                    Networking.Send(state.TheSocket, JsonSerializer.Serialize(powerup) + "\n");
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
                return;

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

            
        }

        private void ProcessMovementRequest(string request, long stateId)
        {
            int snakeId = IdMap[stateId];

            Snake snake = world.snakes[snakeId];

            CtrlCommand? command = JsonSerializer.Deserialize<CtrlCommand>(request);
            if(command == null)
            {
                return;
            }
            // Same direction?

            Vector2D moveRequest = cardinalDirections[command.moving];

            if (!snake.dir.IsOppositeCardinalDirection(moveRequest))
            {
                snake.dir = moveRequest;
                Debug.WriteLine("Changed snake dir to: "+ moveRequest.ToString());
                Console.WriteLine("Changed snake dir to: "+ moveRequest.ToString());
            }
            


        }



        private void UpdateWorld()
        {
            
            lock (world)
            {
                foreach (SocketState state in Clients)
                {

                    // send all objects in the current world,
                    // TODO: maybe copy or move this somewhere?
                    foreach (Snake snake in world.snakes.Values)
                    {
                        // check for collisions, kill snake if needed
                        foreach (Wall wall in world.Walls)
                        {
                            if (snake.RectangleCollision(wall.p1, wall.p2, 25))
                            {
                                // if snake collides with wall, kill it
                                snake.alive = false;
                            }
                        }




                        // if snake is dead, respwan
                        if(!snake.alive) 
                        {
                            if(snake.respawnCounter >= world.RespawnRate)
                            {
                                Random rand = new Random();
                                double coordinate = (rand.NextDouble() * world.UniverseSize) - world.StartingSnakeLength -
                                                    (world.UniverseSize / 2.0);
                                snake.alive = true;
                                snake.respawnCounter = 0;
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

                            snake.MoveSnake(false, world.SnakeSpeed, new Vector2D(0, -1));
                            // snake.MoveSnake(somethings to add)
                            Networking.Send(state.TheSocket, JsonSerializer.Serialize(snake) + "\n");
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