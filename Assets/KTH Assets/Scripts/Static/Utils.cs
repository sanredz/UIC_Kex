using UnityEngine;
using System.Collections;

public class Utils : Singleton<Utils>
{
    private bool gazePointEnabled;
    private GameObject gazePointVisualiser;
    private EyeXGazePointProvider gazePointProvider;
    private Texture2D gazePointTexture;
    private Vector2[] gazePoints;
    private int gazePointIndex;
    private const int NUM_GAZEPOINTS = 50;

    public void Awake()
    {
        gazePointProvider = EyeXGazePointProvider.GetInstance();
        gazePoints = new Vector2[NUM_GAZEPOINTS];
        gazePointTexture = (Texture2D)Resources.Load("GazePoint");
    }

    public bool GetGazePointStatus()
    {
        return gazePointEnabled;
    }

    public void ToggleGazePoint()
    {
        SetGazePoint(!gazePointEnabled);
    }

    public void SetGazePoint(bool enabled)
    {
        gazePointEnabled = enabled;
    }

    public void Pause(bool b)
    {
        Time.timeScale = b ? 0 : 1;
        Screen.lockCursor = !b;
        Cursor.visible = b;
        LockScreen(b);
    }

    public void LockScreen(bool b)
    {
        MouseLook[] mouseLook = GameObject.FindObjectsOfType(typeof(MouseLook)) as MouseLook[];
        foreach (MouseLook m in mouseLook) {
            m.enabled = !b;
        }
    }

    public void CreateFirstPerson()
    {
        GameObject currentCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Settings.camera.tag = "";

        GameObject player = Instantiate(Resources.Load("Player")) as GameObject;
        player.transform.position = Settings.cameraTransform.position;

        UpdateCamera();
        Destroy(currentCamera.transform.root.gameObject);
    }

    public void CreateIntro()
    {
        GameObject currentCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Settings.camera.tag = "";

        GameObject introTween = GameObject.Find("IntroTween");
        if (introTween != null) {
            Destroy(introTween);
        }
        Instantiate(Resources.Load("Intro"));

        UpdateCamera();
        Destroy(currentCamera.transform.root.gameObject);
    }

    public void CreateFreeView()
    {
        GameObject currentCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Settings.camera.tag = "";

        GameObject freeView = Instantiate(Resources.Load("FreeViewCamera")) as GameObject;
        freeView.transform.position = Settings.cameraTransform.position;
        freeView.transform.rotation = Settings.cameraTransform.rotation;

        UpdateCamera();
        Destroy(currentCamera.transform.root.gameObject);
    }

    public void UpdateCamera()
    {
        Settings.camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Settings.cameraTransform = Settings.camera.transform;
    }

    public void OnGUI()
    {
        if (gazePointEnabled) {
            GUI.depth = -2;
            EyeXGazePoint gazePoint = gazePointProvider.GetLastGazePoint(EyeXGazePointType.GazeLightlyFiltered);
            if (!gazePoint.IsWithinScreenBounds || !gazePoint.IsValid) {
                return;
            }
            gazePoints[gazePointIndex] = gazePoint.GUI;
            for (int i = 0; i < NUM_GAZEPOINTS; i++) {
                int textureSize = 6;

                if (i == gazePointIndex) {
                    textureSize = 16;
                }

                Rect pos = new Rect(gazePoints[i].x - textureSize / 2,
                                        gazePoints[i].y - textureSize / 2, textureSize, textureSize);
                GUI.DrawTexture(pos, gazePointTexture);
            }
            gazePointIndex = (gazePointIndex + 1) % NUM_GAZEPOINTS;
        }
    }
}
