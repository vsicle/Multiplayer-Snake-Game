using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerModel
{
    [DataContract(Namespace = "")]
    public class Snake
    {

        [DataMember]
        public int StartingLength;

        //TODO: DataMember SnakeGrowth, an int?
        
        
    }
}
