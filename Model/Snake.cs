using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SnakeGame;

namespace Model
{
   
        
    public class Snake
    {
        [JsonInclude]
        public int snake { get; }
        [JsonInclude]
        public string name { get; }
        [JsonInclude]
        public List<Vector2D> body { get; }
        [JsonInclude]
        public Vector2D dir { get; }
        [JsonInclude]
        public int score { get; }
        [JsonInclude]
        public bool died { get; }
        [JsonInclude]
        public bool alive { get; }
        [JsonInclude]
        public bool dc { get; }
        [JsonInclude]
        public bool join { get; }

        [JsonConstructor]
        public Snake(int snake, string name, List<Vector2D> body, Vector2D dir, int score, bool died, bool alive, bool dc, bool join)
        {
            this.snake = snake;
            this.name = name;
            this.body = body;
            this.dir = dir;
            this.score = score;
            this.died = died;
            this.alive = alive;
            this.dc = dc;
            this.join = join;
        }

        







    }
}
