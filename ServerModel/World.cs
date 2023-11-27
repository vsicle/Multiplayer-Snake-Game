using System.Runtime.Serialization;

namespace ServerModel
{
    [DataContract(Namespace = "")]
    public class World
    {
        [DataMember]
        public int MSPerFrame;

        [DataMember]
        public int RespawnRate;

        [DataMember]
        public int UniverseSize;

        [DataMember]
        public int MaxPowerups { get; set; }

        [DataMember]
        public int PowerupDelay { get; set; }

        [DataMember]
        public int DefaultSnakeSpeed { get; set; }

        [DataMember]
        public List<Wall> Walls;
        
        public World()
        {
            Walls = new List<Wall>();
        }



    }
}