using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SUNCGLoader { 

    public class Config {

	    public const string SUNCGDataPath = "/Users/gabemontague/Courses/FinalProject/SunCG/";
        public const string defaultShader = "Legacy Shaders/Diffuse Fast"; // "OpenGL"
        public const int exportDim = 256;
        //private const string defaultShader = "Unlit/Normals"; // Normals (three channel)
        //private const string defaultShader = "Unlit/NDotV"; // "Camera-illuminated"
        //private const string defaultShader = "Unlit/Depth"; // Depth
        //private const string defaultShader = "Unlit/Albedo"; // Albedo

        public static readonly string[] renderBufferIDs = { 
            "depth",
            "depthb",
            "normals",
            "ndotv",
            "albedo",
            "opengl"
        };
    }
}
