using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
public class Main : MonoBehaviour {

	public enum Method{
		uniformSpawn,
		circleSpawn,
		discSpawn,
		continuousSpawn,
		areaSpawn
	}

	public enum LCPSolutioner {
		mprgp,
		mprgpmic0,
		psor
	}
	public Stopwatch stopWatch = new Stopwatch();
	public float epsilon;
	public int solverMaxIterations;
	public LCPSolutioner solver;
	public Method spawnMethod;

	public float planeSize;
	public int numberOfAgents;

	public bool useGroupedAgents;
	public float individualAgents;
	public float percentOfTwoInGroup;
	public float percentOfThreeInGroup;
	public float percentOfFourInGroup;
	public float agentAvoidanceRadius;
	public float agentMaxSpeed;
	public bool usePresetGroupDistances;
	public float p1p2, p2p3, p3p4;

	public float circleRadius;
	public int numberOfDiscRows;

	public int rows;
	public int rowLength;

	public GameObject agentPrefabs;
	public GameObject groupAgentPrefabs;
	public Agent shirtColorPrefab;
	public bool useSimpleAgents;

	public Grid gridPrefab;
	public Spawner spawnerPrefab;
	public MapGen mapGen;
	public Plane plane;
	internal static Vector2 xMinMax;
	internal static Vector2 zMinMax;
	internal MapGen.map roadmap;

	public int cellsPerRow;
	public int neighbourBins;
	public int roadNodeAmount;
	public bool visibleMap;
	internal float ringDiameter;

	public bool customTimeStep;
	public float timeStep; 

	[Range(0.01f, 1f)]
	public float alpha; 

	List<Agent> agentList = new List<Agent>();

	public bool showSplattedDensity = false;
	public bool showSplattedVelocity = false;
	public bool walkBack = false;
	public bool skipNodeIfSeeNext = false;
	public bool smoothTurns = false;
	public bool handleCollision = false;

	/**
	 * Initialize simulation by taking the user's options into consideration and spawn agents.
	 * Then create the Staggered Grid along with all cells and velocity nodes.
	**/
	void Start () {
		stopWatch.Start();
		UnityEngine.Debug.Log(stopWatch.Elapsed);
		bool error = false; 
		float manx = 0.0f;
//		for (int i = 0; i < agentPrefabs.transform.childCount; ++i) {
//			
//			Mesh mes = agentPrefabs.transform.GetChild (i).GetChild(1).GetComponent<SkinnedMeshRenderer>().sharedMesh;
//			if (agentPrefabs.transform.GetChild (i).gameObject.name == "man-1") {
//				manx = mes.bounds.extents.x;
//				break;
//			}
//		}
//		for (int i = 0; i < agentPrefabs.transform.childCount; ++i) {
//			if (agentPrefabs.transform.GetChild (i).GetComponent<Agent>() == null) {
//				Debug.LogError("The given agent prefab " + agentPrefabs.transform.GetChild(i).name + " is no agent");
//				error = true;
//			}
//			Mesh mes = agentPrefabs.transform.GetChild (i).GetChild(1).GetComponent<SkinnedMeshRenderer>().sharedMesh;
//			if (agentPrefabs.transform.GetChild (i).gameObject.name == "man-1") {
//				
//				Debug.Log (agentPrefabs.transform.GetChild (i).gameObject.name + ": 1.0");
//				continue;
//			}
//
//
//			Debug.Log (agentPrefabs.transform.GetChild (i).gameObject.name + ":" + mes.bounds.extents.x/manx);
//			Debug.Log (mes.bounds.extents.x + " " + mes.bounds.extents.y + " " + mes.bounds.extents.z);
//		}
//
//		for (int i = 0; i < groupAgentPrefabs.transform.childCount; ++i) {
//			if (groupAgentPrefabs.transform.GetChild (i).GetComponent<SubgroupAgent>() == null) {
//				Debug.LogError("The given agent prefab " + groupAgentPrefabs.transform.GetChild(i).name + " is no subgroupagent");
//				error = true;
//			}
//		}
		if (error)
			return;
		
		plane.transform.localScale = new Vector3 (planeSize, 1.0f, planeSize);
		Vector3 planeLength = plane.getLengths (); //Staggered grid length
		xMinMax = new Vector2 (plane.transform.position.x - planeLength.x / 2, 
			                   plane.transform.position.x + planeLength.x / 2);
		zMinMax = new Vector2 (plane.transform.position.z - planeLength.z / 2, 
							  plane.transform.position.z + planeLength.z / 2);

		ringDiameter = agentAvoidanceRadius * 2; //Prefered distance between two agents

		//Creates roadmap / pathfinding for agents based on map
		MapGen m = Instantiate (mapGen) as MapGen; 
		roadmap = m.generateRoadMap (roadNodeAmount, xMinMax, zMinMax, visibleMap);


		Grid grid = Instantiate (gridPrefab) as Grid;
		grid.showSplattedDensity = showSplattedDensity;
		grid.showSplattedVelocity = showSplattedVelocity;
		grid.cellsPerRow = cellsPerRow;
		grid.agentMaxSpeed = agentMaxSpeed;
		grid.ringDiameter = ringDiameter;
		grid.usePresetGroupDistances = usePresetGroupDistances;
		grid.groupDistances = new float[] {p1p2, p2p3, p3p4};
		grid.mapGen = mapGen;
		grid.dt = timeStep; 
		grid.neighbourBins = neighbourBins;
		grid.solver = solver;
		grid.solverEpsilon = epsilon;
		grid.solverMaxIterations = solverMaxIterations;
		grid.colHandler = handleCollision;
		grid.agentAvoidanceRadius = agentAvoidanceRadius;
		Grid.instance = grid;
		Grid.instance.initGrid (xMinMax, zMinMax, alpha, agentAvoidanceRadius);

		for (int i = 0; i < roadmap.spawns.Count; ++i)
			roadmap.spawns [i].spawner.init (ref agentPrefabs, ref groupAgentPrefabs, ref shirtColorPrefab, ref roadmap, 
											 ref agentList, xMinMax, zMinMax, agentAvoidanceRadius, useSimpleAgents,
											 useGroupedAgents, individualAgents, percentOfTwoInGroup, percentOfThreeInGroup, percentOfFourInGroup);
		
		switch(spawnMethod) {
		case Method.uniformSpawn:
			agentList.AddRange (roadmap.spawns [0].spawner.spawnRandomAgents (numberOfAgents));
			break;
		case Method.areaSpawn:
			for (int i = 0; i < roadmap.spawns.Count; ++i)
				agentList.AddRange (roadmap.spawns [i].spawner.spawnAreaAgents (rows, rowLength, roadmap.spawns [i].node));
			break;
		case Method.circleSpawn:
			agentList = spawnerPrefab.circleSpawn (numberOfAgents, circleRadius, planeSize);
			break;
		case Method.discSpawn:
			agentList = spawnerPrefab.discSpawn (planeSize, circleRadius, numberOfDiscRows);
			UnityEngine.Debug.Log ("Spawned: " + agentList.Count + " agents");
			break;
		case Method.continuousSpawn:
			for (int i = 0; i < roadmap.spawns.Count; ++i)
				roadmap.spawns [i].spawner.continousSpawn (roadmap.spawns [i].node, numberOfAgents);
			break;
		default:
			agentList = new List<Agent> (); 
			break;
		}

	}
	

	/**
	 * Main simulation loop which is called every frame
	**/

	void Update () {
		Grid.instance.solver = solver;
		Grid.instance.solverEpsilon = epsilon;
		Grid.instance.solverMaxIterations = solverMaxIterations;

		// Update grid with new density and velocity values
	//	System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
	//	s.Start ();
		Grid.instance.updateCellDensity ();
		Grid.instance.updateVelocityNodes ();
//		Debug.Log ("Update took: " + s.ElapsedMilliseconds + " ms");
		//Solve linear constraint problem
		Grid.instance.PsolveRenormPsolve ();
		//	Debug.Log ("Psolve took: " + s.ElapsedMilliseconds + " ms");
		//Move agents
		int testvar = 0;
		for (int i = 0; i < agentList.Count; ++i) {
			testvar++;
			if (agentList [i].done) {
				Destroy (agentList [i].gameObject);
				//Debug.Log("delete" + testvar);
				//Debug.Log();
				agentList.RemoveAt (i);
				if(agentList.Count == 29 | agentList.Count == 22 | agentList.Count == 14 | agentList.Count == 7 | agentList.Count == 0) {
							UnityEngine.Debug.Log(stopWatch.Elapsed);

				}
				continue;
			}
			//Debug.Log(testvar);

			agentList[i].move(ref roadmap);
		}
		//UnityEngine.Debug.Log("we are done");
	//	Debug.Log ("Agent update took: " + s.ElapsedMilliseconds + " ms");
		//Pair-wise collision handling between agents
		Grid.instance.collisionHandling(ref agentList);
		//	Debug.Log ("Collision handling took: " + s.ElapsedMilliseconds + " ms");

		//flags
		Grid.instance.showSplattedDensity = showSplattedDensity;
		Grid.instance.showSplattedVelocity = showSplattedVelocity;
		Grid.instance.walkBack = walkBack;
		Grid.instance.skipNodeIfSeeNext = skipNodeIfSeeNext;
		Grid.instance.smoothTurns = smoothTurns;

		Grid.instance.dt = customTimeStep ? timeStep : Time.deltaTime;

	}
}
