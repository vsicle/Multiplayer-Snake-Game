using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;
namespace Model
    
{
    /*
     * A JSON Powerup consists of the following fields (names are important)

    "power" - an int representing the powerup's unique ID.
    "loc" - a Vector2D representing the location of the powerup.
    "died" - a bool indicating if the powerup "died" (was collected by a player) on this frame. The server will send the dead   powerups only once.
    The following is an example of the expected format of the JSON for a Powerup object (and hence you must adopt this naming   convention.
    
    {"power":1,"loc":{"x":486.0684871673584,"y":54.912471771240234},"died":false}
     */
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
