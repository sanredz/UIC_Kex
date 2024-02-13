using UnityEngine;
using System.Collections;

public class StartMusicOnLoaded : MonoBehaviour {

	void Update () {
		if (!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play ();
		}
	}
}
