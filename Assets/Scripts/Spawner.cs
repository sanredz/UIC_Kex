using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

	//Common items for this spawner
	internal GameObject agentModels;
	internal GameObject subgroupModels;
	internal List<int> subgroupModelsParentIndex;

	internal Agent manShirtColor;
	internal List<Agent> agentList; //Reference to global agentlist
	internal MapGen.map map; //map of available spawns / goals
	Vector2 X, Z; //Information about plane sizes
	internal float agentAvoidanceRadius;

	internal int tag = 0; //Tag counter for subgroups
	internal Material materialColor; //Material with a color
	internal Material groupAgentMaterial; //Material with a color

	public Color agentSpawnColor = Color.black; //Default color (Changed by user in editor)
	public Color groupAgentSpawnColor = Color.black; //Default color (Changed by user in editor)

	public GameObject agentEditorContainer = null;
	public CustomNode customGoal = null;

	public float spawnRate;
	internal bool useSimpleAgents;

	internal Dictionary<string, int> skins;


	internal bool useGroupedAgents;
	internal float individualAgents;
	internal float percentOfTwoInGroup;
	internal float percentOfThreeInGroup;
	internal float percentOfFourInGroup;

	public void init(ref GameObject agentModels, ref GameObject subgroupModels, ref Agent manShirtColor, 
					 ref MapGen.map map,  ref List<Agent> agentList, Vector2 X, Vector2 Z, float agentAvoidanceRadius, bool useSimpleAgents,
					bool useGroupedAgents, float individualAgents, float percentOfTwoInGroup, float percentOfThreeInGroup, float percentOfFourInGroup) {
		this.agentModels = agentModels;
		this.subgroupModels = subgroupModels;
		this.manShirtColor = manShirtColor;
		this.map = map;
		this.X = X; this.Z = Z;
		this.agentAvoidanceRadius = agentAvoidanceRadius;
		this.agentList = agentList;
		this.useSimpleAgents = useSimpleAgents;
		this.useGroupedAgents = useGroupedAgents;
		this.individualAgents = individualAgents;
		this.percentOfTwoInGroup = percentOfTwoInGroup;
		this.percentOfThreeInGroup = percentOfThreeInGroup;
		this.percentOfFourInGroup = percentOfFourInGroup;

		this.materialColor = Materials.MakeMaterial (agentSpawnColor);
		this.groupAgentMaterial = Materials.MakeMaterial (groupAgentSpawnColor);
		skins = new Dictionary<string, int>();
		Object[] allSkins = Resources.LoadAll ("");
		for (int i = 0; i < allSkins.GetLength (0); ++i) {
			string tag = allSkins [i].name.Split ('-') [0];
			if (skins.ContainsKey (tag)) {
				skins [tag] += 1;
			} else {
				skins.Add (tag, 1);
			}
		}
		subgroupModelsParentIndex = new List<int> ();
		for(int i = 0; i < subgroupModels.transform.childCount; ++i) {
			if (subgroupModels.transform.GetChild (i).tag == "female" || subgroupModels.transform.GetChild (i).tag == "male") {
				subgroupModelsParentIndex.Add (i);
			}
		}
	//	Debug.Log (individualAgents + " " + percentOfTwoInGroup + " " + percentOfThreeInGroup + " " + percentOfFourInGroup);
	}
		
	/**
	 * Spawn a number of regular agents uniformly placed accross the world grid specified by X and Z vectors.
	 * Agents are guaranteed to be spawned on a location not obstructed by a static obstacle.
	 **/
	public List<Agent> spawnRandomAgents(int numberOfAgents) {
		List<Agent> agents = new List<Agent> ();
		int goal = 0;
		if (customGoal == null) {
			Debug.Log ("Please set a goal in the spawner");
			return new List<Agent>();
		}
		//OPT: Use dictionary in mapgen to get constant time access!
		for (int i = 0; i < map.allNodes.Count; ++i) {
			if (map.allNodes [i].transform.position == customGoal.transform.position) {
				goal = i;
				break;
			}
		}

		for (int i = 0; i < numberOfAgents; ++i) {
			Vector3 pos = new Vector3 (Random.Range (X.x, X.y), 10.0f, Random.Range (Z.x, Z.y));
			materialColor = Materials.GetMaterial (Random.Range (0, Settings.numberOfColors)); //Random colors
			while (Physics.Raycast (pos, new Vector3 (0.0f, -1.0f, 0.0f), 20f)) { //Check to see if place occupied by static obstacle
				pos.x = Random.Range (X.x, X.y);
				pos.z = Random.Range (Z.x, Z.y);
			}
			pos.y = 2.0f;
			Agent a = Instantiate (agentModels.transform.GetChild(Random.Range(0, agentModels.transform.childCount)).GetComponent<Agent>()) as Agent;
			a.transform.position = pos;
			float closest = -1;
			int start = -1;
			bool init = false;
			//Find the closest available customNode as a start node. O(n) time where n is the number of nodes in the world.
			for (int j = 0; j < map.allNodes.Count; ++j) {
				if (!Physics.Raycast (pos, (map.allNodes [j].transform.position - pos).normalized, (map.allNodes [j].transform.position-pos).magnitude)) {
					if (map.allNodes[j].transform.position != transform.position && (!init ||  (map.allNodes [j].transform.position-pos).magnitude < closest)) {
						closest = (map.allNodes [j].transform.position - pos).magnitude;
						start = j;
						init = true;
					} 
				}
			}
			if (start < 0 || goal < 0) {
				Debug.Log (a.transform.position);
				Debug.Log ("Insufficient goal nodes in the map. Please place more in empty environments or use a higher spawn-rate of sampled nodes");
				return new List<Agent> ();
			}

			a.path = map.shortestPaths [start] [goal];
			a.pathIndex = 0; //Walk towards first node
			a.preferredVelocity = (map.allNodes[a.path[a.pathIndex]].getTargetPoint(pos) - pos).normalized;

			if (a.transform.childCount > 0) { //Does the agent have a mesh to color?
				Renderer ss = a.transform.GetChild (0).GetComponent<Renderer> ();
				if (ss != null)
					ss.material.mainTexture = (Texture)Resources.Load (a.tag+"-"+Random.Range(0, skins[a.tag]));
			}
			agents.Add (a);
		}
		return agents;
	}

	private void initAgent(ref Agent a, Vector3 pos, int start, int goal, int pathIndex, Material argMat = null) {
		a.transform.position = pos;
		a.transform.right = transform.right;
		a.path = map.shortestPaths [start] [goal]; 
		a.pathIndex = pathIndex;
		a.preferredVelocity = (map.allNodes [a.path [a.pathIndex]].getTargetPoint (a.transform.position) - a.transform.position).normalized;
		if (a.tag == "original") {
			if (a.transform.childCount > 1) {
				a.transform.GetChild(1).GetComponent<SkinnedMeshRenderer> ().sharedMaterial = materialColor;
			}
		} else if (a.transform.childCount > 0) {
			Renderer ss = a.transform.GetChild (0).GetComponent<Renderer> ();
			if (ss != null)
				ss.material.mainTexture = (Texture)Resources.Load (a.tag + "-" + Random.Range (1, skins [a.tag]+1));
			else {
				Renderer ss2 = a.transform.GetChild (1).GetComponent<Renderer> ();
				if (ss2 != null)
					ss2.material.mainTexture = (Texture)Resources.Load (a.tag + "-" + Random.Range (1, skins [a.tag]+1));
			}
			
		}
		if (agentEditorContainer != null)
			a.transform.parent = agentEditorContainer.transform;
	}


	private SubgroupAgent getGroupModel(bool fixedParent, bool leader) {
		SubgroupAgent model;
		if (useSimpleAgents) {
			model = manShirtColor.gameObject.GetComponent<SubgroupAgent> ();
			model.gameObject.GetComponent<Agent> ().enabled = false;
		} else {
			if (fixedParent && leader) {
				model = subgroupModels.transform.GetChild (subgroupModelsParentIndex [Random.Range (0, subgroupModelsParentIndex.Count)]).GetComponent<SubgroupAgent> ();
			} else {
				model = subgroupModels.transform.GetChild (Random.Range (0, subgroupModels.transform.childCount)).GetComponent<SubgroupAgent>();
			}
		}
		return model;
	}
	//Supports 4 followers
	private List<SubgroupAgent> initGroupAgent(int groupSize, Vector3 pos, int start, int goal, int pathIndex, Material argMat = null) {
		bool fixedParent = true;
		List<SubgroupAgent> gr = new List<SubgroupAgent> ();

		SubgroupAgent leader = Instantiate (getGroupModel(fixedParent, true)) as SubgroupAgent;
	
		leader.isLeader = true; leader.transform.position = pos;
		List<Vector3> followerPositions = new List<Vector3> (3); 
		followerPositions.Add (pos);
		float usedValue = 0.6f;//Grid.instance.agentAvoidanceRadius;
		followerPositions.Add (leader.transform.TransformPoint (0.0f, 0.0f, usedValue));
		followerPositions.Add (leader.transform.TransformPoint (0.0f, 0.0f, -usedValue));	
		followerPositions.Add (leader.transform.TransformPoint (0.0f, 0.0f, 2*usedValue));
		followerPositions.Add (leader.transform.TransformPoint (0.0f, 0.0f, -2*usedValue));
		gr.Add (leader);
		for (int i = 0; i < groupSize - 1; ++i) {
			SubgroupAgent follower = Instantiate (getGroupModel(fixedParent, false)) as SubgroupAgent;
			gr.Add (follower);
		}
		SubgroupAgent.companions comp = new SubgroupAgent.companions (gr, 0, transform.gameObject.name + tag.ToString());
		tag++;
		for (int i = 0; i < gr.Count; ++i) {
			gr [i].groupMemberNumber = i; gr [i].number = i;
			gr [i].c = comp;
			Agent sa = gr [i];
			initAgent (ref sa, followerPositions [i], start, goal, pathIndex, groupAgentMaterial);
		}
		return gr;
	}

	/**
	 * Spawn agents centered around this spawner in a rectangular pattern all at once. 
	 * Specified is the percentage of grouped agents compared to individual agents.
	 **/
	public List<Agent> spawnAreaAgents(int rows, int rowLength, int startNode) {
		List<Agent> agents = new List<Agent> ();
		Vector3 startPos = transform.localPosition - (transform.right * rowLength / 2);
		int goal = map.goals[0];
		if (customGoal != null) {
			//OPT: Use dictionary in mapgen to get constant time access!
			for(int i = 0; i < map.allNodes.Count; ++i) {
				if (map.allNodes [i].transform.position == customGoal.transform.position) {
					goal = i;
					break;
				}
			}
		}
		for (int i = 0; i < rows; ++i) {
			for (int j = 0; j < rowLength; ++j) {
				Vector3 posVector = startPos + (transform.right * j) + (transform.right *i);
				posVector.x += 1.5f * j; posVector.z += 1.5f * i; posVector.y = 0.0f;
				int start = startNode;
				//Decides whether to make a normal single agent of a group of 2-3 members
				Agent a = Instantiate (agentModels.transform.GetChild(Random.Range(0, agentModels.transform.childCount)).GetComponent<Agent>()) as Agent;
				initAgent (ref a, posVector, start, goal, 1); //Make agent walk towards next destination
				agents.Add (a);
			}
		}
		return agents;
	}

	internal List<Agent> circleSpawn(int numberOfAgents, float r, float planeScale){
		Color[] colors = {Color.green, Color.yellow, Color.red, Color.magenta, 0.15f*Color.white+Color.blue, Color.cyan};
		Cell tempCell;
		Vector3 agentPos = new Vector3(0f, 0f, 0f);
		if (r > planeScale* 5 - agentAvoidanceRadius) {
			r = planeScale * 5 - agentAvoidanceRadius;
		}

		agentPos.Set(r, 0.5f, 0f);
		float phi = 360 / (float)numberOfAgents;
		List<Agent> li = new List<Agent> ();
		for (int n = 0; n < numberOfAgents; n++) {
			Agent a = Instantiate (manShirtColor) as Agent;

			a.transform.position = agentPos;
			a.transform.RotateAround(new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), (float)(n*phi));
			a.noMap = true;
			a.noMapGoal = new Vector3 (-a.transform.position.x, a.transform.position.y, -a.transform.position.z);
			int index = (int)((n*phi)/60);
			if (a.transform.childCount > 0) {
				SkinnedMeshRenderer smr = a.transform.GetChild (1).GetComponent<SkinnedMeshRenderer> ();
				if (smr != null)
					smr.sharedMaterial = Materials.GetMaterial (index);
			} else {
				MeshRenderer mr = a.GetComponent<MeshRenderer>();
				if(mr != null && mr.materials.GetLength(0) > 0) {
					mr.materials[0].color = Materials.colors[index];
				}
			}
			li.Add (a);
		}
		return li;
	}

	internal List<Agent> discSpawn(float planeScale, float startRadius, int numberOfRows) {
		float r;
		int numberOfAgents;
		float d = 0.4f + agentAvoidanceRadius * 2f;
		List<Agent> li = new List<Agent> ();
		for (int n = 0; n < numberOfRows; n++) {
			r = startRadius+n*agentAvoidanceRadius*2f;
			numberOfAgents = (int)((2*Mathf.PI*r)/d);
			li.AddRange(circleSpawn(numberOfAgents, r, planeScale));
		}
		return li;
	}


	internal IEnumerator spawnContinously(int start, int goal, int cap, float continousSpawnRate) {
		float spawnSizeX = transform.localScale.x;
		float spawnSizeZ = transform.localScale.z;
	
		if (agentList.Count < cap) {
			Vector3 startPos = new Vector3 (Random.Range (-0.5f, 0.5f), 0.15f, Random.Range (-0.5f, 0.5f)); startPos = transform.TransformPoint (startPos);
			float randomRange = Random.Range(0.0f, 1.0f);
			if (!useGroupedAgents || randomRange < individualAgents) {
				Agent a;
				if (useSimpleAgents) {
					a = Instantiate (manShirtColor) as Agent;
				} else {
					a = Instantiate (agentModels.transform.GetChild(Random.Range(0, agentModels.transform.childCount)).GetComponent<Agent>()) as Agent;
				}
				initAgent (ref a, startPos, start, goal, 1);
				agentList.Add (a);
			} else {
				int groupSize;
				if (randomRange - individualAgents < percentOfTwoInGroup) {
					groupSize = 2;
				} else if (randomRange - individualAgents - percentOfTwoInGroup < percentOfThreeInGroup) {
					groupSize = 3;
				} else {
					groupSize = 4;
				}
				List<SubgroupAgent> liA = initGroupAgent (groupSize, startPos, start, goal, 1);
				for (int i = 0; i < liA.Count; ++i) {
					agentList.Add ((Agent)liA [i]);
				}
			}



		//	float agentRelPosRight = Vector3.Dot(a.transform.position - transform.localPosition, transform.right);
		//	float agentRelPosForward = Vector3.Dot(a.transform.position  - transform.localPosition, transform.forward);
		//	a.preferredVelocity = new Vector3 (a.preferredVelocity.x * (-agentRelPosRight / transform.localScale.x), a.preferredVelocity.y, a.preferredVelocity.z * (agentRelPosForward / transform.localScale.z));

		}
		yield return new WaitForSeconds (continousSpawnRate);
		StartCoroutine (spawnContinously(start, goal, cap, continousSpawnRate));
	}

	public void continousSpawn(int startNode, int cap) {
		int goal = map.goals[0];
		if (customGoal != null) {
			//OPT: Use dictionary in mapgen to get constant time access!
			for(int i = 0; i < map.allNodes.Count; ++i) {
				if (map.allNodes [i].transform.position == customGoal.transform.position) {
					goal = i;
					break;
				}
			}
		}
		StartCoroutine (spawnContinously(startNode, goal, cap, spawnRate));
	}

}
