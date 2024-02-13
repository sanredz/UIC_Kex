using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleSpawner : MonoBehaviour {
// OLD SPAWNER
//	public List<SimpleAgent> spawnRandomAgents(ref SimpleAgent originalAgent, int numberOfAgents, Vector2 X, Vector2 Z) {
//		List<SimpleAgent> agents = new List<SimpleAgent> ();
//		for (int i = 0; i < numberOfAgents; ++i) {
//			float xCoord = Random.Range (X.x, X.y);
//			float yCoord = 2.0f;
//			float zCoord = Random.Range (Z.x, Z.y);
//			SimpleAgent a = Instantiate (originalAgent) as SimpleAgent;
//			a.transform.position = new Vector3 (xCoord, yCoord, zCoord);
//			a.transform.Rotate (new Vector3 (0, Random.Range(0, 360), 0));
//			a.velocity = a.transform.forward;
//			agents.Add (a);
//		}
//		return agents;
//	}
}
