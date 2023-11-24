using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;
namespace Model
    
{
    /// Class to represent Powerups in World.
    public class Powerup
    {
        [JsonInclude]
        public int power {get;}

        [JsonInclude]
        public Vector2D loc {get;}

        [JsonInclude]
        public bool died {get;}

        [JsonConstructor]
        public Powerup(int power, Vector2D loc, bool died)
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        }
    }
}
