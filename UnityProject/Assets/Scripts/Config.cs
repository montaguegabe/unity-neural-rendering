using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace SUNCGLoader { 

    public class Config {

        public const string SUNCGDataPath = "/home/pgujjula/Documents/school/college/4_senior_year/sem2/classes/cs184/final_project/SunCG/";
        public const string exportPath    = "/home/pgujjula/Documents/school/college/4_senior_year/sem2/classes/cs184/final_project/SunCG/output/";
        public const string houseID = "fffe2b0adeef7ffb6d3836404de7a640";

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

        public const int startingInd = 0;

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
