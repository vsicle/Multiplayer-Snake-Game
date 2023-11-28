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

        public int SnakeSpeed;

        [DataMember]

        public List<Wall> Walls;

        [DataMember]

        public int StartingSnakeLength;

        [DataMember]

        public int SnakeGrowth;

        public World()
        {
            Walls = new List<Wall>();
            
        }



    }
}