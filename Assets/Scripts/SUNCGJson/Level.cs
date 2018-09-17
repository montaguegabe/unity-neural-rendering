using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SUNCGLoader
{
    [System.Serializable]
    public class Level
    {

        public float id; //Might always be an int, but not sure so leaving as float
        public BBox bbox;
        public Node[] nodes;
    }
}
