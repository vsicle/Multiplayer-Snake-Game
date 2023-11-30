using Model;
using System.Runtime.Serialization;

namespace Model
{
    [DataContract(Namespace = "")]
    public class ServerWorld
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

        public Dictionary<int, Snake> snakes;
        public Dictionary<int, Powerup> powerups;

        public ServerWorld()
        {
            Walls = new List<Wall>();
            snakes = new Dictionary<int, Snake>();
            powerups = new Dictionary<int, Powerup>();
        }



    }
}