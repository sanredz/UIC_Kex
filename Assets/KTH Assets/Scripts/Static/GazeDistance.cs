#define DEBUG
#undef DEBUG
using UnityEngine;
using System.Collections;

public class GazeDistance : Singleton<GazeDistance>
{
    private EyeXGazePoint gazePoint;
    private EyeXGazePointProvider gazePointProvider;
    private Vector3 gazePoint3D;
    private Vector3 gazePoint2D;
    private Vector3 cPos;
    private Rect rect;
    private GameObject latestObject;

    public void Awake()
    {
        gazePointProvider = EyeXGazePointProvider.GetInstance();
    }

    public void Start()
    {
        rect = new Rect();
    }

    public void OnEnable()
    {
        gazePointProvider.StartStreaming(Settings.gazePointType);
    }

    public void OnDisable()
    {
        gazePointProvider.StopStreaming(Settings.gazePointType);
    }

    public void Update()
    {
        if (Settings.triggerOption == TriggerOption.Mouse) {
            gazePoint2D = Input.mousePosition;
        } else {
            gazePoint = gazePointProvider.GetLastGazePoint(Settings.gazePointType);
            gazePoint2D = gazePoint.GUI;
        }
        gazePoint2D.z = 0;
        cPos = Settings.cameraTransform.position;
    }

    public LOD CalculateLOD(GameObject gameObject)
    {
        if (!gameObject.GetComponent<Renderer>().isVisible) {
            return LOD.Minimal;
        }

#if DEBUG
        latestObject = gameObject;
#endif

        Vector3 tPos = gameObject.transform.position;
        float distance = Vector3.Distance(tPos, cPos);

        if (distance >= Settings.Distance.worldMaximum) {
            return LOD.Minimal;
        }

        if (distance >= Settings.Distance.worldMedium) {
            return LOD.Low;
        }

        if (distance <= Settings.Distance.worldMinimum) {
            return LOD.High;
        }

        BoundsToScreenRect(gameObject.GetComponent<Renderer>().bounds);
        distance = DistanceToRectangle() / Settings.diagonalLength;

        if (distance <= Settings.Distance.highestLOD) {
            return LOD.High;
        } else {
            return LOD.Medium;
        }

    }

    private float DistanceToRectangle()
    {
        float dx = Mathf.Max(rect.xMin - gazePoint2D.x, 0, gazePoint2D.x - rect.xMax);
        float dy = Mathf.Max(rect.yMin - gazePoint2D.y, 0, gazePoint2D.y - rect.yMax);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

#if DEBUG
    public void OnGUI()
    {
        if (latestObject != null && latestObject.renderer != null && latestObject.renderer.isVisible) {
            //GUI.Box(ProjectedRect.GetProjectedRect(latestObject, Camera.main).rect, "Distance from trigger option " + CalculateDistance(latestObject));
            BoundsToScreenRect(latestObject.renderer.bounds);
            GUI.Box(rect, "Distance " + CalculateDistance(latestObject));
        }
    }
#endif

    public void BoundsToScreenRect(Bounds b)
    {
        Vector3[] vertices = new Vector3[8];
        Vector3 c = b.center;
        Vector3 e = b.extents;

        // Counter clockwise starting at top most left vertex
        vertices[0] = c + e;
        vertices[1] = new Vector3(c.x + e.x, c.y - e.y, c.z + e.z);
        vertices[2] = new Vector3(c.x - e.x, c.y - e.y, c.z + e.z);
        vertices[3] = new Vector3(c.x - e.x, c.y + e.y, c.z + e.z);
        vertices[4] = new Vector3(c.x + e.x, c.y + e.y, c.z - e.z);
        vertices[5] = new Vector3(c.x + e.x, c.y - e.y, c.z - e.z);
        vertices[6] = new Vector3(c.x - e.x, c.y - e.y, c.z - e.z);
        vertices[7] = new Vector3(c.x - e.x, c.y + e.y, c.z - e.z);

        Vector3 min = ConvertToScreenSpace(vertices[0]);
        Vector3 max = ConvertToScreenSpace(vertices[0]);
        for (int i = 1; i < vertices.Length; i++) {
            vertices[i] = ConvertToScreenSpace(vertices[i]);
            min = Vector3.Min(min, vertices[i]);
            max = Vector3.Max(max, vertices[i]);
        }

        rect.xMin = min.x;
        rect.yMin = min.y;
        rect.xMax = max.x;
        rect.yMax = max.y;
    }

    private Vector3 ConvertToScreenSpace(Vector3 p)
    {
        Vector3 r = Settings.camera.WorldToScreenPoint(p);
        r.y = Screen.height - r.y;
        return r;
    }
}
