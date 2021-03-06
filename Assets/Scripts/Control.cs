﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUNCGLoader;
using System.IO;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour {

    private List<GameObject> cameras = new List<GameObject>();

    static int houseInd = 0;
    static int startHouseInd = 0;
    static int endHouseInd = 100;
    static List<string> houseIds = new List<string>();
    static bool hasFinished = false;

    void LoadCameras(string path) {
        cameras.Clear();
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
            newCamera.tag = "RT";
            newCamera.transform.position = position;
            newCamera.transform.LookAt(position + towards, up);

            Camera cameraComp = newCamera.AddComponent<Camera>();
            cameraComp.enabled = false;
            cameraComp.aspect = 1.0f;
            cameraComp.allowMSAA = true;
            cameraComp.rect = new Rect(0.0f, 0.0f, 256.0f, 256.0f);
            cameraComp.fieldOfView = usedFOV * Mathf.Rad2Deg * 2.0f;
            cameraComp.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
            cameraComp.nearClipPlane = 0.1f;
            cameraComp.farClipPlane = 100.0f;

            cameras.Add(newCamera);

            idx += 1;
        }


        //Debug.Log($"Number of poses: {lines.Length}");
    }

    void ValidateConfig() {
        char last = Config.SUNCGDataPath[Config.SUNCGDataPath.Length - 1];
        Debug.Assert(last == '/');
        char last2 = Config.exportPath[Config.exportPath.Length - 1];
        Debug.Assert(last2 == '/');
    }

    // Render cameras
    // https://docs.unity3d.com/ScriptReference/Camera.Render.html
    void RenderCameras(string houseID)
    {
        const int DIM = Config.exportDim;

        int idx = 0;
        foreach(GameObject camera2 in cameras) {
            Camera cameraComp = camera2.GetComponent<Camera>();

            // TODO: Should I be using 24 bit depth here?
            // TODO: How to make RenderTextureReadWrite so that no transformation is done?

            // Render with each type of shader
            string[] renderBufferIDs = Config.renderBufferIDs;

            foreach (string bufferID in renderBufferIDs) {

                RenderTexture rTex = new RenderTexture(DIM, DIM, 24);
                rTex.antiAliasing = 16;
                rTex.Create();

                RenderTexture rTexOld = RenderTexture.active;
                RenderTexture.active = rTex;

                cameraComp.targetTexture = rTex;

                BufferType bt = BufferType.BufferTypeWithID(bufferID);
                cameraComp.backgroundColor = bt.backgroundColor;
                if (bt.shaderName != "")
                {
                    cameraComp.RenderWithShader(Shader.Find(bt.shaderName), null);
                } else
                {
                    cameraComp.Render();
                }

                // Save to file system
                // TODO: Problem: RGB24 has 8 bits per channel
                // TODO: Do we want linear or SRGB? (last argument)
                Texture2D tex = new Texture2D(DIM, DIM, TextureFormat.RGB24, true, true);
                tex.ReadPixels(new Rect(0, 0, DIM, DIM), 0, 0);
                RenderTexture.active = rTexOld;

                byte[] bytes;
                bytes = tex.EncodeToPNG();

                System.IO.File.WriteAllBytes(
                    $"{Config.exportPath}{houseID}_{idx}_{bufferID}.png", bytes);
                rTex.Release();
            }

            idx += 1;
        }
    }

    private void ClearHouse() {
        cameras.Clear();
        GameObject[] runtimeObjects = GameObject.FindGameObjectsWithTag("RT");
        foreach (GameObject obj in runtimeObjects)
        {
            Object.Destroy(obj);
        }
    }

    private void LoadRender(string houseID)
    {

        string houseJsonPath = $"{Config.SUNCGDataPath}house/{houseID}/house.json";
        string houseCameraPath = $"{Config.SUNCGDataPath}cameras/{houseID}/room_camera.txt";

        if (!File.Exists(houseJsonPath))
        {
            Debug.LogWarning($"No house.json found for house {houseID}... skipping.");
            return;
        }
        if (!File.Exists(houseCameraPath))
        {
            // Don't bother with a warning as this is more common
            return;
        }

        House h = House.LoadFromJson(houseJsonPath);
        Loader l = new Loader();
        l.HouseToScene(h);
        LoadCameras(houseCameraPath);
        RenderCameras(houseID);

    }


    // This method is called for every house!
    void Start()
    {
        ValidateConfig();

        // For testing:
        //   Smallest: "0004d52d1aeeb8ae6de39d6bd993e992";
        //   Broken trees/texture: "00a2a04afad84b16ff330f9038a3d126";
        //LoadAndRender("00a2a04afad84b16ff330f9038a3d126");
        //LoadAndRender("0004d52d1aeeb8ae6de39d6bd993e992");

        // Get houses
        if (houseIds.Count == 0)
        {
            Debug.Log("Fetching big list of house IDs...");
            houseIds = Config.GetHouses();
            Debug.Log("House IDs loaded.");
            int start;
            int end;
            Config.GetRange(out start, out end);
            houseInd = start;
            startHouseInd = start;
            endHouseInd = end;
            Debug.Log($"Starting at index {houseInd}.");
            Debug.Log($"Going until index {end}.");
        }

        if (houseInd >= houseIds.Count || houseInd >= endHouseInd)
        {
            if (!hasFinished)
            {
                Debug.Log("EXPORT COMPLETE!!");

                // Write to a file indicating completion
                string output = $"Completed {startHouseInd} through {endHouseInd}{System.Environment.NewLine}";
                File.AppendAllText($"{Config.SUNCGDataPath}completed.txt", output);
                Application.Quit();

                hasFinished = true;
            }
            return;
        }

        if (houseInd % Config.logEvery == 0)
        {
            Debug.Log($"Rendering house {houseInd}/{houseIds.Count} ({houseInd})");
        }

        // Get the next house
        string houseID = houseIds[houseInd];
        LoadRender(houseID);
        houseInd += 1;

        // Now we clear
        SceneManager.LoadScene("SampleScene");
    }



}
