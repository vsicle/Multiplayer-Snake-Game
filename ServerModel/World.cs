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
        public int MaxPowerups;

        [DataMember]
        public int PowerupDelay;

        [DataMember]
        public int DefaultSnakeSpeed;

        [DataMember]
        public List<Wall> Walls;
        
        public World()
        {
            Walls = new List<Wall>();
        }



    }
}