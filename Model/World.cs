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
        /// update the world, given a JSON string from the server
        /// </summary>
        /// <param name="JsonString"></param>
        public void UpdateWorld(string JsonString)
        {

        }
    }
}