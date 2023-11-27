using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;

namespace ServerModel
{
    [DataContract(Namespace = "")]
    public class Wall
    {
        [DataMember(Order = 0)]
        public int ID { get; }

        [DataMember(Order = 1)]
        public Vector2D p1 { get; }

        [DataMember(Order = 2)]
        public Vector2D p2 { get; }

        public Wall(int _ID, Vector2D p1, Vector2D p2)
        {
            this.ID = _ID;
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
