using Model;
using System.Runtime.Serialization;

namespace Model
{
    [DataContract(Namespace = "")]
    public class ServerWorld : World
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

        public ServerWorld()
        {
            Walls = new List<Wall>();

        }



    }
}