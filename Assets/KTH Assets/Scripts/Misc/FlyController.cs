using UnityEngine;
using System.Collections;

public class FlyController : MonoBehaviour
{
	float f;

    void Update()
    {
		if (Input.GetKey(KeyCode.LeftShift)) {
			f = 10f;
		}
		else if (Input.GetKey(KeyCode.Tab)) {
			f = 0.4f;
		}
		else {
			f = 1f;
		}
        gameObject.transform.Translate((new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) + (Input.GetKey(KeyCode.Space) ? Vector3.up : Vector3.zero)) * Time.deltaTime * 20f * f);
    }
}
