using UnityEngine;
using System.Collections;

public class ShirtColor : MonoBehaviour
{

	public int shirtColor;
	public GameObject characterMesh;
	public GameObject impostor;
	
	
	// Use this for initialization
	void Start()
	{	
		shirtColor = Random.Range(0, Settings.numberOfColors - 1);
		characterMesh.GetComponent<SkinnedMeshRenderer>().sharedMaterial = Materials.GetMaterial(shirtColor);
		impostor.GetComponent<Impostor>().ShirtColor = shirtColor;
	}
}
