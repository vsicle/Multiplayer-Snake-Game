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

namespace ServerController
{
    public class ServerController
    {
        // Map of Clients
        private Dictionary<int, SocketState> Clients;

        private ServerWorld world;
        private int numClients;

        //private string TempPlayerName;

        public ServerController(ServerWorld _world)
        {
            Clients = new Dictionary<int, SocketState>();
            world = _world;

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
                ServerController server = new(XMLWorld);
                server.StartServer();

                Stopwatch sw = new Stopwatch();

                while (true)
                {

                    while (sw.ElapsedMilliseconds < server.world.MSPerFrame) { }

                    sw.Restart();



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

            //List<string> NewPlayerName = BuildIncomingData(state);

            //TempPlayerName = NewPlayerName[0];

           

            state.OnNetworkAction = ConnectedCallBack;
            Networking.GetData(state);

        }

        private void ConnectedCallBack(SocketState state)
        {
            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (Clients)
            {
                numClients++;
                Clients[numClients] = state;
            }

            // TODO: Send PlayerID, but wasn't sure 
            // where we should save PlayerID in Server

            // save player ID in Dictionary?

            Networking.Send(state.TheSocket, numClients + "\n");
            Networking.Send(state.TheSocket, world.UniverseSize.ToString() + "\n");

            List<Vector2D> TempBody = new List<Vector2D>();
            TempBody.Add(new Vector2D(0, -10));
            TempBody.Add(new Vector2D(0, 0));


            Snake NewSnake = new Snake(numClients, "jerry", TempBody, new Vector2D(1, 0), 0, false, true, false, true);
            world.snakes.Add(numClients, NewSnake);

            lock (world)
            {
                foreach (Wall wall in world.Walls)
                {
                    Networking.Send(state.TheSocket, JsonSerializer.Serialize(wall) + "\n");
                }

                // send all objects in the current world,
                // TODO: maybe copy or move this somewhere?
                foreach (Snake snake in world.snakes.Values)
                {
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

            Networking.GetData(state);
        }

        private void UpdateWorld()
        {
            
            lock (world)
            {
                foreach (SocketState state in Clients.Values)
                {
                    foreach (Wall wall in world.Walls)
                    {
                        Networking.Send(state.TheSocket, JsonSerializer.Serialize(wall) + "\n");
                    }

                    // send all objects in the current world,
                    // TODO: maybe copy or move this somewhere?
                    foreach (Snake snake in world.snakes.Values)
                    {
                        Networking.Send(state.TheSocket, JsonSerializer.Serialize(snake) + "\n");
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