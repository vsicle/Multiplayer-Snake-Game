using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model
{
    /*
     * Control commands are how the client will tell the server what it wants to do (what direction it wants to move). 

    A control command consists of the following field (names are important)
    
    "moving" - a string representing whether the player wants to move or not, and the desired direction. Possible values are:   "none", "up", "left", "down", "right".
    {"moving":"left"}
     */
    public class CtrlCommand
    {
        [JsonInclude]
        private string moving { get; set; }

        [JsonConstructor]
        public CtrlCommand(string moving)
        {
            this.moving = moving;
        }
    }
}
