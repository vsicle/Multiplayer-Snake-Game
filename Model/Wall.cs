using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model
{
    /// Class to represent wall objects in World.
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
