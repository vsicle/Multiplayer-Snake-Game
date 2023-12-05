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
        public List<Vector2D> body { get; set; }

        
        [JsonInclude]
        public Vector2D dir { get; set; }
        [JsonInclude]
        public int score { get; }
        [JsonInclude]
        public bool died { get; }
        [JsonInclude]
        public bool alive { get; set; }
        // Variable to indicate if Snake Client disconnected
        // from server.
        [JsonInclude]
        public bool dc { get; }
        // Variable indicating if player joined on a frame
        // (only true one frame).
        [JsonInclude]
        public bool join { get; }

        public int respawnCounter { get; set; }

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
            respawnCounter = 0;
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

        public void MoveSnake(bool DirectionChange, double speed, Vector2D dir)
        {
            //if (!DirectionChange)
            //{
            //    this.body[body.Count - 1] = (this.dir * speed) + this.body[body.Count - 1];
            //}
            //else
            //{
            //    // Change direction
            //    this.dir = dir;
            //    Vector2D NewSnakeHead = (this.dir * speed) + this.body[body.Count - 1];
            //    this.body.Add(NewSnakeHead);

            //}




            // moving head logic 
            if (this.body[body.Count - 1].X != this.body[body.Count - 2].X)
            {
                // Check to see if snake head is going left or right.

                // This is going right
                if (this.body[body.Count - 1].X > this.body[body.Count - 2].X)
                {

                    this.body[body.Count - 1].X += speed;

                }

                // If head is going left
                else
                {

                    this.body[body.Count - 1].X -= speed;

                }

            }
            // If head segment is vertical
            else
            {

                // Check to see if head is heading down.
                if (this.body[body.Count - 1].Y > this.body[body.Count - 2].Y)
                {
                    this.body[body.Count - 1].Y += speed;

                }
                else
                {
                    this.body[body.Count - 1].Y -= speed;

                }
            }





            // Tail logic

            // If tail segment is horizontal

            if (this.body[0].X != this.body[1].X)
            {
                // Check to see if snake is headed left or right.

                // This is going right
                if (this.body[0].X < this.body[1].X)
                {

                    this.body[0].X += speed;

                    // If end of tail segment passes start of tail
                    if (this.body[0].X >= this.body[1].X)
                    {
                        this.body[1].X = this.body[0].X;
                        // Remove end of tail.
                        this.body.RemoveAt(0);
                    }
                }

                // If tail is going left
                else
                {

                    this.body[0].X -= speed;

                    if (this.body[0].X <= this.body[1].X)
                    {
                        this.body[1].X = this.body[0].X;
                        // Remove end of tail.
                        this.body.RemoveAt(0);
                    }

                }

            }

            // If tail segment is vertical
            else
            {

                // Check to see if tail is heading down.
                if (this.body[0].Y < this.body[1].Y)
                {
                    this.body[0].Y += speed;

                    if (this.body[0].Y >= this.body[1].Y)
                    {
                        this.body[1].Y = this.body[0].Y;
                        // Remove end of tail.
                        this.body.RemoveAt(0);
                    }

                }
                else
                {
                    this.body[0].Y -= speed;

                    if (this.body[0].Y <= this.body[1].Y)
                    {
                        this.body[1].Y = this.body[0].Y;
                        // Remove end of tail.
                        this.body.RemoveAt(0);
                    }

                }


            }
        }



    }
}
