using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour
{

    public new GameObject camera;
    public Hashtable myTween;

    private Transform cityTarget;
	public GameObject cameraTarget;

    private float pathPosition;
    void Start()
    {
        //cityTarget = GameObject.Find("CityBlock").transform;
		cityTarget = cameraTarget.transform;
        Path1();

    }
    private void Path1()
    {
        myTween = new Hashtable();
        myTween.Add("looktarget", cityTarget);
        myTween.Add("time", 15f);
        myTween.Add("looktime", 0f);
        myTween.Add("path", iTweenPath.GetPath("Path1"));
        myTween.Add("easetype", iTween.EaseType.linear);
        myTween.Add("looptype", iTween.LoopType.none);
        myTween.Add("oncomplete", "Path2");
        myTween.Add("oncompletetarget", gameObject);
        iTween.MoveTo(camera, myTween);
    }

    private void Path2()
    {
        myTween = new Hashtable();
        myTween.Add("looktarget", cityTarget);
        myTween.Add("time", 40f);
        myTween.Add("looktime", 0f);
        myTween.Add("path", iTweenPath.GetPath("Path2"));
        myTween.Add("easetype", iTween.EaseType.linear);
        myTween.Add("looptype", iTween.LoopType.none);
        myTween.Add("oncomplete", "Path3");
        myTween.Add("oncompletetarget", gameObject);
        iTween.MoveTo(camera, myTween);
    }

    private void Path3()
    {
        myTween = new Hashtable();
        myTween.Add("looktarget", cityTarget);
        myTween.Add("time", 30f);
        myTween.Add("looktime", 0f);
        myTween.Add("path", iTweenPath.GetPath("Path3"));
        myTween.Add("easetype", iTween.EaseType.linear);
        myTween.Add("looptype", iTween.LoopType.none);
        myTween.Add("oncomplete", "Path1");
        myTween.Add("oncompletetarget", gameObject);
        iTween.MoveTo(camera, myTween);
    }
}