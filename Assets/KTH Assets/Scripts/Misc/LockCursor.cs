using UnityEngine;
using System.Collections;

public class LockCursor : MonoBehaviour
{
    public bool crosshairEnabled = true;
    public Texture crosshair = null;
    public float scale = 0.5f;

    private Rect position;

    void Start()
    {
        Cursor.visible = false;
        Screen.lockCursor = true;
        if (crosshair != null)
        {
            int width = (int)(crosshair.width * scale);
            int height = (int)(crosshair.height * scale);
            position = new Rect((Screen.width - width) / 2 - width / 2, (Screen.height - height) / 2 - height / 2,
            width, height);
        } 
    }

    void OnGUI()
    {
        if (crosshairEnabled && position != null)
        {
            GUI.DrawTexture(position, crosshair, ScaleMode.ScaleToFit);
        }
    }
}
