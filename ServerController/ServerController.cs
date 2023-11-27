using ServerModel;
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
            using (XmlReader reader = XmlReader.Create("WorldSettings.xml", settings))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(World));
                World? theWorld = (World?)serializer.ReadObject(reader);
                if (theWorld == null)
                {
                    Console.WriteLine("World is null, possibly incorrect world settings xml format");
                }
                
            }

        }
    }
}