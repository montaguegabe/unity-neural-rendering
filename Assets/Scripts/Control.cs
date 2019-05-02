using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUNCGLoader;
using System.IO;
using System;

public class Control : MonoBehaviour {

    void LoadCameras(string path) {
        string txtContents = File.ReadAllText(path);
        string[] lines = txtContents.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.RemoveEmptyEntries
        );

        int idx = 0;
        foreach (string line in lines) {

            // See: https://github.com/shurans/SUNCGtoolbox/blob/4322dcf88c6ac82ef7471e76eea5ebd9db4e4f04/gaps/apps/scn2img/scn2img.cpp?fbclid=IwAR1DJ_HR4bOWjpCaYk_dmC15ucraFViGitQGWxzvk5DGYbafoF7gV7L3Sno#L235
            var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float vx = float.Parse(parts[0]);
            float vy = float.Parse(parts[1]);
            float vz = float.Parse(parts[2]);
            float tx = float.Parse(parts[3]);
            float ty = float.Parse(parts[4]);
            float tz = float.Parse(parts[5]);
            float ux = float.Parse(parts[6]);
            float uy = float.Parse(parts[7]);
            float uz = float.Parse(parts[8]);
            float xf = float.Parse(parts[9]);
            float yf = float.Parse(parts[10]); // we should entirely discard this value to match original code

            // Value seems like it is a measure of the camera's "goodness" of 
            // view it presents
            float value = float.Parse(parts[11]);

            Vector3 position = new Vector3(vx, vy, vz);
            Vector3 towards = (new Vector3(tx, ty, tz)).normalized;
            Vector3 up = (new Vector3(ux, uy, uz)).normalized;

            // TODO: Revise for non-square rendering: assumes square
            float usedFOV = xf;

            GameObject newCamera = new GameObject($"Camera_{idx}");
            Camera cameraComp = newCamera.AddComponent<Camera>();
            cameraComp.aspect = 1.0f;
            cameraComp.fieldOfView = usedFOV * Mathf.Rad2Deg; 
            newCamera.transform.position = position;

            //TODO: Set near clip and far clip based on scene

            newCamera.transform.LookAt(position + towards, up);
            idx += 1;
        }


        Debug.Log($"Number of poses: {lines.Length}");
    }

    void ValidateConfig() {
        char last = Config.SUNCGDataPath[Config.SUNCGDataPath.Length - 1];
        Debug.Assert(last == '/');
    }

    // Render cameras
    // https://docs.unity3d.com/ScriptReference/Camera.Render.html
    void RenderCameras()
    {
        //TODO:
    }

    void Start()
    {
        ValidateConfig();

        string houseID = "0a41dad983c48b40618a56cd5772ff97";

        string houseJsonPath = $"{Config.SUNCGDataPath}house/{houseID}/house.json";
        string houseCameraPath = $"{Config.SUNCGDataPath}cameras/{houseID}/room_camera.txt";

        House h = House.LoadFromJson(houseJsonPath);
        Loader l = new Loader();
        l.HouseToScene(h);
        LoadCameras(houseCameraPath);
    }
}
