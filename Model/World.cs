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
        private Dictionary<int, Snake> snakes;
        private Dictionary<int, Powerup> powerups;
        private Dictionary<int, Wall> walls;
        private int size;

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
        /// update the world, given a JSON string from the server
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
