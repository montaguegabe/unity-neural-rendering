using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUNCGLoader;
using System.IO;

public class Control : MonoBehaviour {

    private List<GameObject> cameras = new List<GameObject>();

    void LoadCameras(string path) {
        string txtContents = File.ReadAllText(path);
        string[] lines = txtContents.Split(
            new[] { "\r\n", "\r", "\n" },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        int idx = 0;
        foreach (string line in lines) {

            // See: https://github.com/shurans/SUNCGtoolbox/blob/4322dcf88c6ac82ef7471e76eea5ebd9db4e4f04/gaps/apps/scn2img/scn2img.cpp?fbclid=IwAR1DJ_HR4bOWjpCaYk_dmC15ucraFViGitQGWxzvk5DGYbafoF7gV7L3Sno#L235
            var parts = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
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
            newCamera.transform.position = position;
            newCamera.transform.LookAt(position + towards, up);

            Camera cameraComp = newCamera.AddComponent<Camera>();
            cameraComp.enabled = false;
            cameraComp.aspect = 1.0f;
            cameraComp.allowMSAA = true;
            cameraComp.fieldOfView = usedFOV * Mathf.Rad2Deg * 2.0f;
            cameraComp.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
            cameraComp.nearClipPlane = 0.1f;
            cameraComp.farClipPlane = 100.0f;

            cameras.Add(newCamera);

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
        int idx = 0;
        foreach(GameObject camera in cameras) {

            const int DIM = Config.exportDim;

            Camera cameraComp = camera.GetComponent<Camera>();

            // TODO: Should I be using 24 bit depth here?
            // TODO: How to make RenderTextureReadWrite so that no transformation is done?

            RenderTexture rTex = new RenderTexture(DIM, DIM, 24);
            rTex.antiAliasing = 4;
            rTex.Create();

            RenderTexture rTexOld = RenderTexture.active;
            RenderTexture.active = rTex;

            cameraComp.targetTexture = rTex;
            cameraComp.RenderWithShader(Shader.Find(Config.defaultShader), null);
            //cameraComp.Render();

            // Save to file system
            // TODO: Problem: RGB24 has 8 bits per channel
            // TODO: Do we want linear or SRGB? (last argument)
            Texture2D tex = new Texture2D(DIM, DIM, TextureFormat.RGB24, true, true);
            tex.ReadPixels(new Rect(0, 0, DIM, DIM), 0, 0);
            RenderTexture.active = rTexOld;

            byte[] bytes;
            bytes = tex.EncodeToPNG();

            System.IO.File.WriteAllBytes(
                $"{Config.SUNCGDataPath}output/render_{idx}.png", bytes);
            rTex.Release();

            idx += 1;
        }
    }

    void Start()
    {
        ValidateConfig();

        // For testing:
        //   Smallest: "0004d52d1aeeb8ae6de39d6bd993e992";
        //   Broken trees/texture: "00a2a04afad84b16ff330f9038a3d126";
        string houseID = "0004d52d1aeeb8ae6de39d6bd993e992"; 

        string houseJsonPath = $"{Config.SUNCGDataPath}house/{houseID}/house.json";
        string houseCameraPath = $"{Config.SUNCGDataPath}cameras/{houseID}/room_camera.txt";

        House h = House.LoadFromJson(houseJsonPath);
        Loader l = new Loader();
        l.HouseToScene(h);
        LoadCameras(houseCameraPath);
        RenderCameras();

    }
}
