using ServerModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace ServerController
{
    public class ServerController
    {
        

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            World? theWorld;
            using (XmlReader reader = XmlReader.Create("WorldSettings.xml", settings))
            {
                
                DataContractSerializer serializer = new DataContractSerializer(typeof(World));
                theWorld = (World?)serializer.ReadObject(reader);
                if (theWorld == null)
                {
                    Console.WriteLine("World is null, possibly incorrect world settings xml format");
                }
                
            }
            
            if (theWorld != null)
            {
                while (theWorld.MaxPowerups > 100 || theWorld.SnakeSpeed > 9 || theWorld.StartingSnakeLength > 360 || theWorld.SnakeGrowth > 600)
                {
                    Console.WriteLine("A default value in the XML settings is too large. Decrease MaxPowerups, DefaultSnakeSpeed or " +
                        "StartingSnakeLength, close, and restart Server");
                }




            }

            

        }
    }
}