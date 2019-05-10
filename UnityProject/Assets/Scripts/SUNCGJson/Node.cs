using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SUNCGLoader
{
    [System.Serializable]
    public class Node
    {
        public string id;
        public string type;
        public int valid; //0 or 1, not sure if built in deserializer will cast appropriately so leaving as int
        public string modelId;
        public Material[] materials;
        //dimensions, spec lists this but I've never seen it
        public float[] transform; //ObjToScene coords 4x4
        public int isMirrored; //0 or 1, flip normals
        public string[] roomTypes;
        public int[] nodeIndicies;
    }
}
