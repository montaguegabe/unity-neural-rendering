using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace SUNCGLoader { 

    public class Config {

	    public const string SUNCGDataPath = "/Users/gabemontague/Courses/FinalProject/SunCG/";
        public const string exportPath = "/Users/gabemontague/Courses/FinalProject/SunCG/output/";

        // This shader is used for "OpenGL" rendering
        public const string defaultShader = "Legacy Shaders/Diffuse Fast"; // Simple look no shadow

        public const int exportDim = 256;

        public static readonly string[] renderBufferIDs = { 
            "depth",
            "depthb",
            "normals",
            "ndotv",
            "albedo",
            "opengl"
        };


        // Fetch a list of houseIDs to render
        public static List<string> GetHouses()
        {
            List<string> lines = File.ReadLines($"{SUNCGDataPath}houses.txt").ToList();
            return lines;
        }

        // End is not inclusive
        public static void GetRange(out int start, out int end)
        {
            List<string> lines = File.ReadLines($"{SUNCGDataPath}range.txt").ToList();
            Debug.Assert(lines.Count >= 2);
            start = System.Convert.ToInt32(lines[0]);
            end = System.Convert.ToInt32(lines[1]);
        }

        // How many houses between Debug.Log'ing the status
        public const int logEvery = 10;

        // Uncomment for debugging
        //public static List<string> getHouses()
        //{
        //    return new List<string> {
        //    "00a2a04afad84b16ff330f9038a3d126",
        //    "0004d52d1aeeb8ae6de39d6bd993e992" 
        //    };
        //}
    }
}
