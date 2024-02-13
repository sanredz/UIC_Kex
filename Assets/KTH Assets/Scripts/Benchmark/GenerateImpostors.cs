using UnityEngine;
using System.Collections;

public class GenerateImpostors : MonoBehaviour
{

    private GameObject character;
    private const int offset = 2;
    private const int t = 40;
    private const int impostors = t * t;

    public void Start()
    {
        character = (GameObject)Resources.Load("Character");
        int x = -t;
        int z = t;

        for (int i = 0; i < impostors; i++) {
            GameObject g = (GameObject)Instantiate(character);
            g.GetComponent<CharacterAnimation>().NormalizedTime = Random.Range(0f, 1f);
            g.transform.position = new Vector3(x, g.transform.position.y, z);
            if (x == t) {
                x = -t;
                z -= offset;
            }
            x += offset;
            g.transform.Rotate(new Vector3(0, Random.Range(0f, 360f), 0));
        }

        Debug.Log("Number of impostors: " + impostors);
    }

}
