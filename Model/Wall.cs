using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Wall
    {

        public int wall {get;set;}
        public Vector2D p1 { get;set;}
        public Vector2D p2 {get;set;}

        public Wall(int _wallId, Vector2D _p1, Vector2D _p2)
        {
            wall = _wallId;
            p1 = _p1;
            p2 = _p2;
        }
    }

    
}
