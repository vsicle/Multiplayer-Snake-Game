using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model
{
    /*
     * A JSON Wall consists of the following fields (names are important)

    "wall" - an int representing the wall's unique ID.
    "p1" - a Vector2D representing one endpoint of the wall.
    "p2" - a Vector2D representing the other endpoint of the wall.
    You can assume the following about all walls:
    
    They will always be axis-aligned (purely horizontal or purely vertical, never diagonal). This means p1 and p2 will have either  the same x value or the same y value.
    The length between p1 and p2 will always be a multiple of the wall width (50 units).
    The endpoints of the wall can be anywhere (not just multiples of 50), as long as the distance between them is a multiple of 50.
    The order of p1 and p2 is irrelevant (they can be top to bottom, bottom to top, left to right, or right to left).
    Walls can overlap and intersect each other.
    The following is an example of the expected format of the JSON for a Wall object (and hence you must adopt this naming  convention.
    
    {"wall":1,"p1":{"x":-575.0,"y":-575.0},"p2":{"x":-575.0,"y":575.0}}
     */
    public class Wall
    {
        [JsonInclude]
        public int wall { get; }

        [JsonInclude]
        public Vector2D p1 { get;}

        [JsonInclude]
        public Vector2D p2 {get;}

        [JsonConstructor]
        public Wall(int wall, Vector2D p1, Vector2D p2)
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;
        }
    }

    
}
