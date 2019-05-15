using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUNCGLoader;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.InteropServices;

public class Control : MonoBehaviour {

    private List<GameObject> cameras = new List<GameObject>();

    static int houseInd = 0;
    static List<string> houseIds = new List<string>();


    private RenderTexture albedoRTex;
    private RenderTexture depthRTex;
    private RenderTexture normalsRTex;

    public Texture2D albedoTex;
    public Texture2D depthTex;
    public Texture2D normalsTex;
    public Texture2D outputTex;

    GCHandle albedoPin;
    GCHandle depthPin;
    GCHandle normalsPin;
    bool pinSet = false;

    GameObject mainCamera;
    Camera mainCameraComp;

    [DllImport("RenderingPlugin")]
    private static extern void SetTimeFromUnity(float t);

    [DllImport("RenderingPlugin")]
    private static extern void SetRenderingOn(bool t);

    [DllImport("RenderingPlugin")]
    private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);

    [DllImport("RenderingPlugin")]
    private static extern void SetInputTexturesFromUnity(System.IntPtr albedo,
        System.IntPtr depth, System.IntPtr normals);

    [DllImport("RenderingPlugin")]
    private static extern IntPtr GetRenderEventFunc();

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
            //newCamera.tag = "MainCamera";
            newCamera.transform.position = position;
            newCamera.transform.LookAt(position + towards, up);

            Camera cameraComp = newCamera.AddComponent<Camera>();
            cameraComp.enabled = false;
            cameraComp.aspect = 1.0f;
            cameraComp.rect = new Rect(0.0f, 0.0f, 256.0f, 256.0f);
            cameraComp.fieldOfView = usedFOV * Mathf.Rad2Deg * 2.0f;
            cameraComp.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
            cameraComp.nearClipPlane = 0.1f;
            cameraComp.farClipPlane = 100.0f;
            cameraComp.allowMSAA = true;

            cameras.Add(newCamera);
            mainCamera = newCamera;
            mainCameraComp = cameraComp;

            idx += 1;

            // Only load a single camera
            break;
        }
    }

    void ValidateConfig() {
        char last = Config.SUNCGDataPath[Config.SUNCGDataPath.Length - 1];
        Debug.Assert(last == '/');
        char last2 = Config.exportPath[Config.exportPath.Length - 1];
        Debug.Assert(last2 == '/');
    }

    // Render cameras
    // https://docs.unity3d.com/ScriptReference/Camera.Render.html
    void RenderCamera()
    {

        // Render with each type of shader
        string[] renderBufferIDs = Config.renderBufferIDs;
        int idx = 0;
        foreach (string bufferID in renderBufferIDs)
        {
            RenderTexture rTex;
            Texture2D tex;
            if (idx == 0)
            {
                rTex = albedoRTex;
                tex = albedoTex;
            } else if (idx == 1)
            {
                rTex = depthRTex;
                tex = depthTex;
            } else
            {
                rTex = normalsRTex;
                tex = normalsTex;
            }

            RenderTexture.active = rTex;
            mainCameraComp.targetTexture = rTex;

            BufferType bt = BufferType.BufferTypeWithID(bufferID);
            mainCameraComp.backgroundColor = bt.backgroundColor;
            mainCameraComp.RenderWithShader(Shader.Find(bt.shaderName), null);
            tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            tex.Apply(); // TODO: Can remove?

            idx += 1;
        }

    }

    private void LoadHouse(string houseID)
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

    }


    // This method is called for every house!
    IEnumerator Start()
    {
        ValidateConfig();
        Cursor.visible = false;

        // For testing:
        //   Smallest: "0004d52d1aeeb8ae6de39d6bd993e992";
        //   Broken trees/texture: "00a2a04afad84b16ff330f9038a3d126";
        //LoadAndRender("00a2a04afad84b16ff330f9038a3d126");
        //LoadAndRender("0004d52d1aeeb8ae6de39d6bd993e992");

        // Create textures (TODO: 24 slows things down?)
        albedoRTex = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
        depthRTex = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
        normalsRTex = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
        albedoRTex.antiAliasing = 8;
        depthRTex.antiAliasing = 8;
        normalsRTex.antiAliasing = 8;

        // TODO: Can get away with RGB24?
        albedoTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        depthTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        normalsTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);

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
            Debug.Log($"Starting at index {houseInd}.");
        }

        // Get the next house
        string houseID = houseIds[houseInd];
        LoadHouse(houseID);

        // Create output texture
        outputTex = new Texture2D(256, 256, TextureFormat.RGB24, false);
        outputTex.Apply();
        SetTextureFromUnity(outputTex.GetNativeTexturePtr(), outputTex.width, outputTex.height);

        yield return StartCoroutine("CallPluginAtEndOfFrames");
    }

    private IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();

            // Set time for the plugin
            SetTimeFromUnity(Time.timeSinceLevelLoad);

            // Issue a plugin event with arbitrary integer identifier.
            // The plugin can distinguish between different
            // things it needs to do based on this ID.
            // For our simple plugin, it does not matter which ID we pass here.
            GL.IssuePluginEvent(GetRenderEventFunc(), 1);
        }
    }

    float horizontalSpeed = 2.2f;
    float verticalSpeed = 2.2f;
    float lat = 0.0f;
    float lng = 90.0f;

    int viewMode = 1;
    private void Update()
    {
        // Exit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            viewMode = 1;
            SetRenderingOn(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            viewMode = 2;
            SetRenderingOn(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            viewMode = 3;
            SetRenderingOn(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            viewMode = 4;
            SetRenderingOn(true);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            viewMode = 5;
            SetRenderingOn(true);
        }

        // Move
        Vector3 update = Vector3.zero;
        const float moveSpeed = 2.5f;
        if (Input.GetKey(KeyCode.W))
        {
            update += mainCamera.transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            update -= mainCamera.transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            update -= mainCamera.transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            update += mainCamera.transform.right;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            update += Vector3.down;
        }
        if (Input.GetKey(KeyCode.E))
        {
            update += Vector3.up;
        }
        update.Normalize();
        mainCamera.transform.position += update * moveSpeed * Time.deltaTime;

        // Look
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        lng += h;
        lat -= v;
        mainCamera.transform.rotation = Quaternion.Euler(lat, lng, 0.0f);

        // Render
        RenderCamera();
        byte[] albedoB = albedoTex.GetRawTextureData();
        byte[] depthB = depthTex.GetRawTextureData();
        byte[] normalsB = normalsTex.GetRawTextureData();

        if (pinSet)
        {
            albedoPin.Free();
            depthPin.Free();
            normalsPin.Free();
        }


        albedoPin = GCHandle.Alloc(albedoB, GCHandleType.Pinned);
        depthPin = GCHandle.Alloc(depthB, GCHandleType.Pinned);
        normalsPin = GCHandle.Alloc(normalsB, GCHandleType.Pinned);
        pinSet = true;

        // Pass input bufers to the plugin
        SetInputTexturesFromUnity(albedoPin.AddrOfPinnedObject(),
            depthPin.AddrOfPinnedObject(),
            normalsPin.AddrOfPinnedObject());
    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            float dim = Mathf.Min(Screen.width, Screen.height);
            float halfX = (Screen.width - dim) / 2.0f;
            float halfY = (Screen.height - dim) / 2.0f;
            switch (viewMode)
            {
                case 1:
                    Graphics.DrawTexture(new Rect(halfX, halfY, dim, dim), albedoTex);
                    break;
                case 2:
                    Graphics.DrawTexture(new Rect(halfX, halfY, dim, dim), depthTex);
                    break;
                case 3:
                    Graphics.DrawTexture(new Rect(halfX, halfY, dim, dim), normalsTex);
                    break;
                case 4:
                    Graphics.DrawTexture(new Rect(halfX, halfY, dim, dim), outputTex);
                    break;
                default:
                    float halfDim = dim * 0.5f;
                    Graphics.DrawTexture(new Rect(halfX, halfY, halfDim, halfDim), albedoTex);
                    Graphics.DrawTexture(new Rect(halfX + halfDim, halfY, halfDim, halfDim), depthTex);
                    Graphics.DrawTexture(new Rect(halfX, halfY + halfDim, halfDim, halfDim), normalsTex);
                    Graphics.DrawTexture(new Rect(halfX + halfDim, halfY + halfDim, halfDim, halfDim), outputTex);
                    break;
            }
        }
    }
}
