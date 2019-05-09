using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUNCGLoader;
using System.IO;
using UnityEngine.SceneManagement;

public class FlyCamera : MonoBehaviour {
 
    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/
     
     
    float mainSpeed = 0.4f; //regular speed
    float shiftAdd = 0.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 1000.0f; //Maximum speed when holdin gshift
    float camSens = 0.25f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun= 1.0f;
     
    void Update () {
      /*
        lastMouse = Input.mousePosition - lastMouse ;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0 );
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse =  Input.mousePosition;
      */
        //Mouse  camera angle done.  
       
        //Keyboard commands
        float f = 0.0f;
        Vector3 p = GetBaseInput();
        Vector3 q = GetYRotation();
        q = q * 0.3f;
        transform.Rotate(q);
        Vector3 r = GetXRotation();
        r = r * 0.3f;
        transform.Rotate(r, Space.World);


        p = p * mainSpeed;
       
        p = p * Time.deltaTime;

        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space)){ //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else{
            transform.Translate(p);
        }
       
    }
     
    private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey (KeyCode.W)){
            p_Velocity += new Vector3(0, 0 , 1);
        }
        if (Input.GetKey (KeyCode.S)){
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey (KeyCode.A)){
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey (KeyCode.D)){
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }

    private Vector3 GetYRotation() { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_RotationalVelocity = new Vector3();
        if (Input.GetKey (KeyCode.J)){
            p_RotationalVelocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey (KeyCode.K)){
            p_RotationalVelocity += new Vector3(1, 0, 0);
        }


        return p_RotationalVelocity;
    }

    private Vector3 GetXRotation() {
        Vector3 p_RotationalVelocity = new Vector3();
        if (Input.GetKey (KeyCode.H)){
            p_RotationalVelocity += new Vector3(0, -1, 0);
        }
        if (Input.GetKey (KeyCode.L)){
            p_RotationalVelocity += new Vector3(0, 1, 0);
        }

        return p_RotationalVelocity;
    }
}

public class Control : MonoBehaviour {

    private List<GameObject> cameras = new List<GameObject>();

    static int houseInd = Config.startingInd;
    static List<string> houseIds = new List<string>();
    static bool hasFinished = false;

    void LoadCameras(string path) {
        cameras.Clear();

        foreach (Camera camera in Camera.allCameras) {
          if (string.Equals(camera.name, "Main Camera")) {
            camera.cullingMask = -1; // Everything
          }
        }

        string txtContents = File.ReadAllText(path);
        string[] lines = txtContents.Split(
            new[] { "\r\n", "\r", "\n" },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        string line = lines[0];

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

        GameObject newCamera = new GameObject($"Camera_0");
        newCamera.tag = "MainCamera";
        newCamera.transform.position = position;
        newCamera.transform.LookAt(position + towards, up);

        GameObject newCamera2 = new GameObject($"Camera_1");
        newCamera2.tag = "MainCamera";
        newCamera2.transform.position = position;
        newCamera2.transform.LookAt(position + towards, up);

        FlyCamera fc = newCamera.AddComponent<FlyCamera>();
        Camera cameraComp = newCamera.AddComponent<Camera>();
        cameraComp.enabled = true;
        cameraComp.gameObject.SetActive(true);
        cameraComp.aspect = 1.0f;
        cameraComp.allowMSAA = true;
        cameraComp.rect = new Rect(0.0f, 0.0f, 256.0f, 256.0f);
        cameraComp.fieldOfView = usedFOV * Mathf.Rad2Deg * 2.0f;
        cameraComp.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
        cameraComp.nearClipPlane = 0.1f;
        cameraComp.farClipPlane = 100.0f;

        cameras.Add(newCamera);

        fc = newCamera2.AddComponent<FlyCamera>();
        cameraComp = newCamera2.AddComponent<Camera>();
        cameraComp.enabled = true;
        cameraComp.gameObject.SetActive(true);
        cameraComp.aspect = 1.0f;
        cameraComp.allowMSAA = true;
        cameraComp.rect = new Rect(0.0f, 0.0f, 256.0f, 256.0f);
        cameraComp.fieldOfView = usedFOV * Mathf.Rad2Deg * 2.0f;
        cameraComp.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
        cameraComp.nearClipPlane = 0.1f;
        cameraComp.farClipPlane = 100.0f;

        cameras.Add(newCamera2);

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

        GameObject camera2 = cameras[1];
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
                $"{Config.exportPath}{houseID}_{Time.frameCount}_{bufferID}.png", bytes);
            rTex.Release();
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
        Application.targetFrameRate = 24;

        // For testing:
        //   Smallest: "0004d52d1aeeb8ae6de39d6bd993e992";
        //   Broken trees/texture: "00a2a04afad84b16ff330f9038a3d126";
        //LoadRender("00a2a04afad84b16ff330f9038a3d126");
        LoadRender("0004d52d1aeeb8ae6de39d6bd993e992");
        hasFinished = true;
        return;

        // Get houses
        /*
        if (houseIds.Count == 0)
        {
            Debug.Log("Fetching big list of house IDs...");
            houseIds = Config.GetHouses();
            Debug.Log("House IDs loaded.");
            Debug.Log($"Starting at index {houseInd}.");
        }

        if (houseInd >= houseIds.Count)
        {
            if (!hasFinished)
            {
                Debug.Log("EXPORT COMPLETE!!");
                hasFinished = true;
            }
            return;
        }

        if (houseInd % 1 == 0)
        {
            Debug.Log($"Rendering house {houseInd}/{houseIds.Count} ({houseInd})");
        }

        // Get the next house
        string houseID = houseIds[houseInd];
        LoadRender(houseID);
        houseInd += 1;

        // Now we clear
        SceneManager.LoadScene("SampleScene");
    */
    }

    void Update() {
        RenderCameras("0004d52d1aeeb8ae6de39d6bd993e992");
    }
}
