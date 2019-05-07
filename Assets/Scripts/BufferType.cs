using System;
using UnityEngine;

public struct BufferType
{
    public string shaderName;
    public Color backgroundColor;

    static Color defaultSkyColor = new Color(0.1921569f, 0.3019608f, 0.4745098f);

    static public BufferType BufferTypeWithID(string id)
    {
        switch (id)
        {
            case "depth":
                return new BufferType
                {
                    shaderName = "Unlit/Depth",
                    backgroundColor = Color.white
                };
            case "depthb":
                return new BufferType
                {
                    shaderName = "Unlit/Depth",
                    backgroundColor = Color.black
                };
            case "normals":
                return new BufferType
                {
                    shaderName = "Unlit/Normals",
                    backgroundColor = Color.black
                };
            case "ndotv":
                return new BufferType
                {
                    shaderName = "Unlit/NDotV",
                    backgroundColor = Color.white // sky is illuminated thus white
                };
            case "albedo":
                return new BufferType
                {
                    shaderName = "Unlit/Albedo",
                    backgroundColor = defaultSkyColor
                };
            case "opengl":
                return new BufferType
                {
                    shaderName = "",
                    backgroundColor = defaultSkyColor
                };
            default:
                throw new System.ArgumentException("Invalid buffer type ID");
        }
    }
}
