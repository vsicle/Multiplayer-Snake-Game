using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Class to represent wall objects in World.
    /// </summary>
    [DataContract(Namespace = "")]
    public class Wall
    {
        [JsonInclude]
        [DataMember(Order = 0)]
        public int wall { get; set; }

        [JsonInclude]
        [DataMember(Order = 1)]
        public Vector2D p1 { get; set; }

        [JsonInclude]
        [DataMember(Order = 2)]
        public Vector2D p2 { get; set; }

        [JsonConstructor]
        public Wall(int wall, Vector2D p1, Vector2D p2)
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;
        }
        /// <summary>
        /// In case JSON Constructor isn't happy.
        /// </summary>
        //public Wall()
        //{
        //    wall = 0;
        //    p1 = new Vector2D();
        //    p2 = new Vector2D();
        //}

        /// <summary>
        /// Backup in case JSON Serialization fails.
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return "{\"wall\":" + wall.ToString() + ",\"p1\":{\"x\":" + p1.GetX().ToString() + ",\"y\":" + p1.GetY().ToString() + "},\"p2\":{\"x\":" + p2.GetX().ToString() + ",\"y\":" + p2.GetY().ToString() + "}}";
        }
    }

    
}
