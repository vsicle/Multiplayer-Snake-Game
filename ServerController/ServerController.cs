using ServerModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using NetworkUtil;
using System.Net.Sockets;

namespace ServerController
{
    public class ServerController
    {
        // Map of Clients
        private Dictionary<long, SocketState> Clients;

        private World world;


        public ServerController(World _world)
        {
            Clients = new Dictionary<long, SocketState>();
            world = _world;
            
        }

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            World? XMLWorld;
            using (XmlReader reader = XmlReader.Create("WorldSettings.xml", settings))
            {
                
                DataContractSerializer serializer = new DataContractSerializer(typeof(World));
                XMLWorld = (World?)serializer.ReadObject(reader);
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
            }

            
            

        }

        /// <summary>
        /// Start accepting Tcp sockets connections from clients
        /// </summary>
        private void StartServer()
        {
            // This begins an "event loop"
            Networking.StartServer(NewClientConnected, 11000);
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

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (Clients)
            {
                Clients[state.ID] = state;
            }

            // TODO: Send PlayerID, but wasn't sure 
            // where we should save PlayerID in Server
            //Networking.Send(state.TheSocket, );
            Networking.Send(state.TheSocket, world.UniverseSize.ToString() + "\n");

            foreach (Wall wall in world.Walls)
            {
                Networking.Send(state.TheSocket, wall.ToString() + "\n");
            }

            // TODO:
            // change the state's network action to the 
            // receive handler so we can process data when something
            // happens on the network
            //state.OnNetworkAction = ReceiveMessage;

            Networking.GetData(state);
        }


    }
}