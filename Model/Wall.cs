using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Wall
    {

        public int WallId {get;set;}
        public Vector2D P1 {get;set;}
        public Vector2D P2 {get;set;}

        public Wall(int _wallId, Vector2D _p1, Vector2D _p2)
        {
            WallId = _wallId;
            P1 = _p1;
            P2 = _p2;
        }
    }

    
}
