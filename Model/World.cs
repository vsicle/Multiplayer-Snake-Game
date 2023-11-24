using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Model

{
    /// <summary>
    /// This class contains all the objects the Server sends.
    /// </summary>
    public class World
    {
        public Dictionary<int, Snake> snakes;
        public Dictionary<int, Powerup> powerups;
        public Dictionary<int, Wall> walls;
        // World size to use for grid coordinates.
        public int size { get; }

        /// <summary>
        /// create a blank world
        /// </summary>
        public World()
        {
            snakes = new Dictionary<int, Snake>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
        }

        /// <summary>
        /// create a blank world
        /// </summary>
        public World(int worldSize)
        {
            snakes = new Dictionary<int, Snake>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
            size = worldSize;
        }

        /// <summary>
        /// Method to update the world, given a JSON string from the server
        /// </summary>
        /// <param name="JsonString"></param>

        public void UpdateWorld(string JsonString)
        {

            // figure out what object is being created or updated
            // likely using the first word in the JSON
            JsonDocument JDoc = JsonDocument.Parse(JsonString);


            if (JDoc.RootElement.TryGetProperty("snake", out _))
            {
                Snake? tempSnake = JsonSerializer.Deserialize<Snake>(JsonString);
                if (tempSnake != null)
                {

                    if (snakes.ContainsKey(tempSnake.snake))
                    {
                        if (tempSnake.dc)
                        {
                            snakes.Remove(tempSnake.snake);
                            return;
                        }
                        snakes[tempSnake.snake] = tempSnake;
                    }
                    else
                    {
                        snakes.Add(tempSnake.snake, tempSnake);
                    }
                }
            }
            else if (JDoc.RootElement.TryGetProperty("wall", out _))
            {
                Wall? tempWall = JsonSerializer.Deserialize<Wall>(JsonString);
                if (tempWall != null)
                {
                    if (walls.ContainsKey(tempWall.wall))
                    {
                        walls[tempWall.wall] = tempWall;
                    }
                    else
                    {
                        walls.Add(tempWall.wall, tempWall);
                    }
                }
            }
            else if (JDoc.RootElement.TryGetProperty("power", out _))
            {
                Powerup? tempPower = JsonSerializer.Deserialize<Powerup>(JsonString);
                if (tempPower != null)
                {
                    if (powerups.ContainsKey(tempPower.power))
                    {
                        if (tempPower.died)
                        {
                            powerups.Remove(tempPower.power);
                            return;
                        }
                        powerups[tempPower.power] = tempPower;
                    }
                    else
                    {
                        powerups.Add(tempPower.power, tempPower);
                    }
                }
            }
        }


    }
}
