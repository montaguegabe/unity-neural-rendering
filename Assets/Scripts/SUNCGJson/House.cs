using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace SUNCGLoader
{
    [System.Serializable]
    public class House
    {
        public string id;
        public float[] front; //Is a 3-float vector
        public float[] up;
        public float scaleToMeters;
        public Level[] levels;

        public static House LoadFromJson(string path)
        {
            House h = JsonUtility.FromJson<House>(File.ReadAllText(path));
            Debug.Log($"Number of levels: {h.levels.Length}");
            return h;
        }

        public static GameObject HouseToScene(House house)
        {
            GameObject root = new GameObject("House_" + house.id);
            foreach(Level level in house.levels)
            {
                LoadLevel(root, level);
            }
            return root;
        }

        private static void LoadLevel(GameObject root, Level level)
        {
            GameObject levelRoot = new GameObject("Level_" + level.id);
            levelRoot.transform.parent = root.transform;
            foreach(Node n in level.nodes)
            {
                LoadNode(levelRoot, n);
            }

        }

        private static void LoadNode(GameObject levelRoot, Node node)
        {
            GameObject nodeObj = new GameObject("Node_" + node.id);

        }
    }
}
