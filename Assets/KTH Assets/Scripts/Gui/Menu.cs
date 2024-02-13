using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{
    private bool showMenu;
    private GUIStyle textStyle;

    private int topDefault = 10;
    private int top = 10;
    private int left = 20;
    private int width = 200;
    private int height = 20;
    private int menuOffset = 30;
    private int items = 11;

    public void Awake()
    {
        textStyle = new GUIStyle();
        textStyle.normal.textColor = Color.white;
        textStyle.alignment = TextAnchor.MiddleCenter;
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Toggle();
        }
    }

    protected virtual void OnGUI()
    {
        Event e = Event.current;
        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) {
            showMenu = false;
            Utils.Instance.Pause(showMenu);
            return;
        }
        if (showMenu) {
            DrawMenu();
        }
    }

    private void Toggle()
    {
        showMenu = !showMenu;
        Utils.Instance.Pause(showMenu);
    }

    protected virtual void DrawMenu()
    {
        GUI.depth = -1;

        // Background box
        GUI.BeginGroup(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - width, Screen.width, Screen.height));
        GUI.Box(new Rect(0, top, width + left * 2, items * menuOffset), "");
        GUI.Label(new Rect(left, top, width, height), "Controllers", textStyle);

        if (GUI.Button(new Rect(left, (top += menuOffset), width, height), "First person")) {
            Utils.Instance.CreateFirstPerson();
            Toggle();
        }

        if (GUI.Button(new Rect(left, (top += menuOffset), width, height), "Free camera view")) {
            Utils.Instance.CreateFreeView();
            Toggle();
        }

        if (GUI.Button(new Rect(left, (top += menuOffset), width, height), "Movie view")) {
            Utils.Instance.CreateIntro();
            Toggle();
        }

        GUI.Label(new Rect(left, (top += 2 * menuOffset), width, height), "Options", textStyle);

        if (GUI.Button(new Rect(left, (top += menuOffset), width, height), "Display gaze point: " + Utils.Instance.GetGazePointStatus())) {
            Utils.Instance.ToggleGazePoint();
        }

        GUI.Label(new Rect(left, (top += menuOffset), width, height), "Number of impostors", textStyle);

        string text = GUI.TextField(new Rect(2 * left, (top += menuOffset), width - left * 2, height), Settings.numberOfImpostors.ToString());
        int number;
        if (int.TryParse(text, out number) && number >= Settings.minImpostors && number <= Settings.maxImpostors) {
            Settings.numberOfImpostors = number;
        }

        if (GUI.Button(new Rect(left, (top += menuOffset * 2), width, height), "Exit game")) {
            Application.Quit();
        }

        GUI.EndGroup();
        top = topDefault;
    }
}
