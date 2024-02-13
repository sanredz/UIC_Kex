using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
    public int FramesPerSec { get; protected set; }
    public float updateFrequency = 0.5f;

    private string text;
    private GUIStyle style;
    private Rect pos;

    private void Start()
    {
        int w = Screen.width, h = Screen.height;
        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 18;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        pos = new Rect(5, 5, w, h * 2 / 100);
        StartCoroutine(FPS());
    }

    private IEnumerator FPS()
    {
        for (;;) {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(updateFrequency);
            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;
            
            FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
            text = "~ " + FramesPerSec.ToString() + " fps";
        }
    }
    void OnGUI()
    {
        GUI.Label(pos, text, style);
    }
}