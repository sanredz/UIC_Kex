//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Visualizes the gaze point in the game window using a tiny GUI.Box.
/// </summary>
public class GazePointVisualizer : MonoBehaviour
{
    private EyeXGazePointProvider _gazePointProvider;

#if UNITY_EDITOR
    private EyeXGazePointType _oldGazePointType;
#endif

    /// <summary>
    /// Choice of gaze point data stream to be visualized.
    /// </summary>
    public EyeXGazePointType gazePointType = EyeXGazePointType.GazeLightlyFiltered;

    /// <summary>
    /// The size of the visualizer point
    /// </summary>
    public float pointSize = 5;

    /// <summary>
    /// The color of the visualizer point
    /// </summary>
    public Color pointColor = Color.yellow;

    public void Awake()
    {
        _gazePointProvider = EyeXGazePointProvider.GetInstance();

#if UNITY_EDITOR
        _oldGazePointType = gazePointType;
#endif
    }

    public void OnEnable()
    {
        _gazePointProvider.StartStreaming(gazePointType);
    }

    public void OnDisable()
    {
        _gazePointProvider.StopStreaming(gazePointType);
    }

    /// <summary>
    /// Draw a GUI.Box at the user's gaze point.
    /// </summary>
    public void OnGUI()
    {
#if UNITY_EDITOR
        if (_oldGazePointType != gazePointType)
        {
            _gazePointProvider.StopStreaming(_oldGazePointType);
            _oldGazePointType = gazePointType;
            _gazePointProvider.StartStreaming(gazePointType);
        }
#endif

        var defaultStyle = GUI.skin.box;
        GUI.skin.box = CreateBoxStyle();

        var title = "";

        // Show fixation index for fixation types
        if (gazePointType == EyeXGazePointType.FixationSensitive || gazePointType == EyeXGazePointType.FixationSlow)
        {
            var fixationIndex = _gazePointProvider.GetLastFixationCount(gazePointType);
            title = fixationIndex.ToString();
        }

        var gazePoint = _gazePointProvider.GetLastGazePoint(gazePointType);
        if (gazePoint.IsWithinScreenBounds)
        {
            GUI.Box(new UnityEngine.Rect(gazePoint.GUI.x - pointSize / 2.0f, gazePoint.GUI.y - pointSize / 2.0f, pointSize, pointSize), title);
        }

        GUI.skin.box = defaultStyle;
    }

    private GUIStyle CreateBoxStyle()
    {
        var style = new GUIStyle(GUI.skin.box);

        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, pointColor);
        texture.Apply();
        style.normal.background = texture;

        style.border = new RectOffset(0, 0, 0, 0);

        return style;
    }
}
