using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;
namespace Model
    
{
    /// Class to represent Powerups in World.
    public class Powerup
    {
        [JsonInclude]
        public int power {get;}

        [JsonInclude]
        public Vector2D loc {get;}

        [JsonInclude]
        public bool died { get; set; }

        [JsonConstructor]
        public Powerup(int power, Vector2D loc, bool died)
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
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
                    return DoubleAscendingOrder(Loc1.X - RectWidth - 5.0, loc.X, Loc2.X + RectWidth + 5.0) &&
                        DoubleAscendingOrder(Loc1.Y - RectWidth - 5.0, loc.Y, Loc1.Y + RectWidth + 5.0);
                }
                // If Loc2 is on left.
                return DoubleAscendingOrder(Loc2.X - RectWidth - 5.0, loc.X, Loc1.X + RectWidth + 5.0) &&
                    DoubleAscendingOrder(Loc1.Y - RectWidth - 5.0, loc.Y, Loc1.Y + RectWidth + 5.0);
                // If it is a vertical segment.
            }
            else
            {
                // If Loc1 is on top
                if (Loc1.Y < Loc2.Y)
                {
                    return DoubleAscendingOrder(Loc1.Y - RectWidth - 5.0, loc.Y, Loc2.Y + RectWidth + 5.0) &&
                        DoubleAscendingOrder(Loc1.X - RectWidth - 5.0, loc.X, Loc1.X + RectWidth + 5.0);
                }
                // If Loc2 is on top.
                return DoubleAscendingOrder(Loc2.Y - RectWidth - 5.0, loc.Y, Loc1.Y + RectWidth + 5.0) &&
                    DoubleAscendingOrder(Loc1.X - RectWidth - 5.0, loc.X, Loc1.X + RectWidth + 5.0);
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
