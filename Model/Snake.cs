using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SnakeGame;

namespace Model
{
   
    ///  Class to represent Snakes in World.       
    public class Snake
    {
        [JsonInclude]
        public int snake { get; }
        [JsonInclude]
        public string name { get; }
        [JsonInclude]
        public List<Vector2D> body { get; }
        [JsonInclude]
        public Vector2D dir { get; set; }
        [JsonInclude]
        public int score { get; }
        [JsonInclude]
        public bool died { get; }
        [JsonInclude]
        public bool alive { get; }
        // Variable to indicate if Snake Client disconnected
        // from server.
        [JsonInclude]
        public bool dc { get; }
        // Variable indicating if player joined on a frame
        // (only true one frame).
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

        /// <summary>
        /// Method to detect snake & powerup collisions.
        /// </summary>
        /// <param name="PowerUpLoc"> Provided powerup location </param>
        /// <returns>True if snake head & powerup location are within drawing range, false otherwise </returns>
        public bool PowerUpCollision(Vector2D PowerUpLoc)
        {
            if ((PowerUpLoc - this.body[body.Count-1]).Length() <= 26.0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// A helper method to determine if this snake head is within
        /// another snake segment or wall boundary.
        /// </summary>
        /// <param name="Loc1">One of two Snake or Wall coordinates.</param>
        /// <param name="Loc2">One of two Snake or Wall coordinates.</param>
        /// <param name="RectWidth">Snake or Wall width.</param>
        /// <returns>True if snake head is within boundary, false otherwise.</returns>

        public bool RectangleCollision(Vector2D Loc1, Vector2D Loc2, double RectWidth)
        {
            // Check if it is a horizontal segment.
            if (Loc1.X != Loc2.X)
            {
                // If Loc1 is on left.
                if (Loc1.X < Loc2.X)
                {
                    return DoubleAscendingOrder(Loc1.X - RectWidth - 10.0, this.body[body.Count - 1].X, Loc2.X + RectWidth + 10.0) && 
                        DoubleAscendingOrder(Loc1.Y - RectWidth - 10.0, this.body[body.Count - 1].Y, Loc1.Y + RectWidth + 10.0);
                }
                // If Loc2 is on left.
                return DoubleAscendingOrder(Loc2.X - RectWidth - 10.0, this.body[body.Count - 1].X, Loc1.X + RectWidth + 10.0) && 
                    DoubleAscendingOrder(Loc1.Y - RectWidth - 10.0, this.body[body.Count - 1].Y, Loc1.Y + RectWidth + 10.0);
            // If it is a vertical segment.
            } else
            {
                // If Loc1 is on top
                if (Loc1.Y < Loc2.Y)
                {
                    return DoubleAscendingOrder(Loc1.Y - RectWidth - 10.0, this.body[body.Count - 1].Y, Loc2.Y + RectWidth + 10.0) &&
                        DoubleAscendingOrder(Loc1.X - RectWidth - 10.0, this.body[body.Count - 1].X, Loc1.X + RectWidth + 10.0);
                }
                // If Loc2 is on top.
                return DoubleAscendingOrder(Loc2.Y - RectWidth - 10.0, this.body[body.Count - 1].Y, Loc1.Y + RectWidth + 10.0) &&
                    DoubleAscendingOrder(Loc1.X - RectWidth - 10.0, this.body[body.Count - 1].X, Loc1.X + RectWidth + 10.0);
            }
        }

        /// <summary>
        /// Helper method to determine if three double
        /// values are in ascending order.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <param name="three"></param>
        /// <returns>True if one <= two <= three, false otherwise.</returns>

        private bool DoubleAscendingOrder(double one, double two, double three)
        {
            if ((one <= two) && (two <= three))
            {
                return true;
            }
            return false;   
        }





    }
}
