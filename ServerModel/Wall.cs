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
        public int ID { get; set; }

        [DataMember(Order = 1)]
        public Vector2D p1 { get; set; }

        [DataMember(Order = 2)]
        public Vector2D p2 { get; set; }

        public Wall(int _ID, Vector2D p1, Vector2D p2)
        {
            this.ID = _ID;
            this.p1 = p1;
            this.p2 = p2;
        }

        public override String ToString()
        {
            return "{\"wall\":" + ID.ToString() + ",\"p1\":{\"x\":" + p1.GetX().ToString() + ",\"y\":" + p1.GetY().ToString() + "},\"p2\":{\"x\":" + p2.GetX().ToString() + ",\"y\":" + p2.GetY().ToString() + "}}";
        }
    }
}
