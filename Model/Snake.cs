using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    
    public class Snake
    {
        
        public Snake(string _name, string _ID)
        {
            Name = _name;
            ID = _ID;
        }

        public string Name { get; }
        public string ID { get; }
    }
}
