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

            //TODO: fix JSON issue

            var JsonName = JDoc.RootElement.EnumerateObject();
            string objectKind = JsonName.ElementAt(0).Name;

            // TA said to use rootelement.TryGetValue


            switch(objectKind)
            {
                case "wall":
                    Wall? tempWall = JsonSerializer.Deserialize<Wall>(objectKind);
                    if(tempWall != null)
                    {
                        walls.Add(tempWall.wall, tempWall);
                    }
                    break;
                case "snake":
                    Snake? tempSnake = JsonSerializer.Deserialize<Snake>(objectKind);
                    if (tempSnake != null)
                    {
                        snakes.Add(tempSnake.snake, tempSnake);
                    }
                    break;
                case "power":
                    Powerup? tempPower = JsonSerializer.Deserialize<Powerup>(objectKind);
                    if (tempPower != null)
                    {
                        powerups.Add(tempPower.power, tempPower);
                    }
                    break;
                default:
                    Debug.WriteLine("Failed to match/find type of JSON object");
                    break;
            }
            

        


        }
    }
}