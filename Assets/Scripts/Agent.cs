using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {
	public Vector3 preferredVelocity, continuumVelocity, collisionAvoidanceVelocity;
	public Vector3 velocity;
	public List<int> path;
	internal int pathIndex = 0;
	internal float agentRelXPos, agentRelZPos;
	internal float neighbourXWeight, neighbourZWeight, neighbourXZWeight, selfWeight;
	internal float selfRightVelocityWeight, selfLeftVelocityWeight, selfUpperVelocityWeight, selfLowerVelocityWeight, 
	neighbourRightVelocityWeight, neighbourLeftVelocityWeight, neighbourUpperVelocityWeight, neighbourLowerVelocityWeight;
	internal float densityAtAgentPosition;

	internal bool done = false;
	internal bool noMap = false;
	internal Vector3 noMapGoal;
	internal Animator animator;
	internal Rigidbody rbody;
	internal bool collision = false;
	internal int row,column;
	Vector3 prevPos;
	Vector3 previousDirection;

	internal void Start() {
		animator = transform.gameObject.GetComponent<Animator> ();
		rbody = transform.gameObject.GetComponent<Rigidbody> ();
		//Which cell am i in currently?
		calculateRowAndColumn();
		if (!Grid.instance.colHandler && rbody != null) {
			Destroy (rbody);
		}
	}

	internal void calculateRowAndColumn() {
		row = (int)((transform.position.z - Main.zMinMax.x)/Grid.instance.cellLength); 
		column = (int)((transform.position.x - Main.xMinMax.x)/Grid.instance.cellLength); 
		if (row < 0)
			row = 0; 
		if (column < 0)
			column = 0;
		if (row > Grid.instance.cellsPerRow - 1) {
			row = Grid.instance.cellsPerRow - 1;
		}
		if (column > Grid.instance.cellsPerRow - 1) {
			column = Grid.instance.cellsPerRow - 1;
		}
		agentRelXPos = transform.position.x - Grid.instance.cellMatrix [row, column].transform.position.x;
		agentRelZPos = transform.position.z - Grid.instance.cellMatrix [row, column].transform.position.z;
	}

	/**
	 * Calculate the actual velocity of this agent, based on continuum, preferred and collision avoidance velocities
	 **/ 
	internal void setCorrectedVelocity() {
		calculateDensityAtPosition ();
		calculateContinuumVelocity ();
		//-1 since we subtract this agents density at position
		velocity = preferredVelocity + ((densityAtAgentPosition - 1 / Mathf.Pow (Grid.instance.cellLength, 2)) / Grid.maxDensity)
		* (continuumVelocity - preferredVelocity);
		velocity.y = 0f;
		transform.forward = velocity.normalized;
		velocity = velocity + collisionAvoidanceVelocity;
	}

	internal bool canSeeNext(ref MapGen.map map, int modifier) {
		if (pathIndex + modifier< path.Count && pathIndex + modifier >= 0 && pathIndex + modifier < map.allNodes.Count) {
			//Can we see next goal?
			Vector3 next = map.allNodes[path[pathIndex+modifier]].getTargetPoint(transform.position);
			Vector3 dir = next - transform.position;
			if(!Physics.Raycast (transform.position, dir.normalized, dir.magnitude)) {
				return true;
			}
		}
		return false;
	}
	/**
	 * Calculate the preferred velocity by looking at desired path
	 **/ 
	bool change = false;
	internal void calculatePreferredVelocityMap(ref MapGen.map map) {
		previousDirection = preferredVelocity.normalized;
		if ((transform.position - map.allNodes [path [pathIndex]].getTargetPoint(transform.position)).magnitude < map.allNodes[path[pathIndex]].getThreshold() || (Grid.instance.skipNodeIfSeeNext && canSeeNext(ref map, 1))) {
			//New node reached
			collision = false;
			pathIndex += 1;
			if (pathIndex >= path.Count) {
				//Done
				done = true;
			} else {
				Vector3 nextDirection = ((map.allNodes [path [pathIndex]].getTargetPoint(transform.position)) - transform.position).normalized;
				if (Vector3.Angle (previousDirection, nextDirection) > 20.0f && Grid.instance.smoothTurns) {
					preferredVelocity = Vector3.RotateTowards (velocity.normalized, nextDirection, Grid.instance.dt*((35.0f - 400*Grid.instance.dt) * Mathf.PI / 180.0f), 15.0f).normalized;
					change = true;
				}
			}
		} else if(pathIndex > 0 && Grid.instance.walkBack && !canSeeNext(ref map, 0)) { //Can we see current heading? Are we trapped?
			//No. We want to go back
			preferredVelocity = ((map.allNodes [path [pathIndex-1]].getTargetPoint(transform.position)) - transform.position).normalized;
			change = false;
		} else {
			collision = false;
			Vector3 nextDirection = ((map.allNodes [path [pathIndex]].getTargetPoint(transform.position)) - transform.position).normalized;
			if (change && Vector3.Angle (previousDirection, nextDirection) > 20.0f && Grid.instance.smoothTurns) {
				preferredVelocity = Vector3.RotateTowards (velocity.normalized, nextDirection, Grid.instance.dt*((35.0f - 400*Grid.instance.dt) * Mathf.PI / 180.0f),  15.0f).normalized;
			} else {
				change = false;
				preferredVelocity = ((map.allNodes [path [pathIndex]].getTargetPoint (transform.position)) - transform.position).normalized;
			}
		}
		//collision = false;
		preferredVelocity = preferredVelocity * Grid.instance.agentMaxSpeed;
		preferredVelocity.y = 0f;
	}

	/**
	 * Calculate the preferred velocity of a single uncharted point as a goal 
	 **/
	internal void calculatePreferredVelocityNoMap() {
		if ((transform.position - noMapGoal).magnitude < MapGen.DEFAULT_THRESHOLD) {
			//New node reached
			//Done
			done = true;
		} else {
			preferredVelocity = (noMapGoal - transform.position).normalized;
		}
		preferredVelocity = preferredVelocity * Grid.instance.agentMaxSpeed;
		preferredVelocity.y = 0f;
	}

	internal virtual void calculatePreferredVelocity(ref MapGen.map map) {
		if (noMap) {
			calculatePreferredVelocityNoMap ();
		} else {
			calculatePreferredVelocityMap (ref map);
		}
	}
	/**
	 * Change the position of the agent and reset variables. 
	 * Do animations.
	 **/
	internal void changePosition(ref MapGen.map map) {
		if (done) {
			return; // Dont do anything
		} 
		calculatePreferredVelocity(ref map);

		setCorrectedVelocity ();
		prevPos = transform.position;

		Vector3 nextPos = transform.position + velocity * Grid.instance.dt; nextPos.y = 3.0f;

		transform.position += velocity * Grid.instance.dt;


		if(rbody != null)
			rbody.velocity = Vector3.zero;


		collisionAvoidanceVelocity = Vector3.zero;

		float realSpeed = Vector3.Distance (transform.position, prevPos) / Mathf.Max(Grid.instance.dt, Time.deltaTime);
		if (animator != null) {
	
			if (realSpeed < 0.05f) {
				animator.speed = 0;
			} else {
				animator.speed = (realSpeed) / Grid.instance.agentMaxSpeed;
			}
		}
	}

	internal void OnCollisionEnter(Collision c) {
		collision = true;
	}
	/**
	 * Do a bilinear interpolation of surrounding densities and come up with a density at this agents position.
	 **/
	internal void calculateDensityAtPosition() {
		densityAtAgentPosition = 0.0f;
		int xNeighbour = (int)(column + neighbourXWeight/Mathf.Abs(neighbourXWeight));	//Column for the neighbour which the agent contributes to
		int zNeighbour = (int)(row + neighbourZWeight/Mathf.Abs(neighbourZWeight));		//Row for the neighbour which the agent contributes to

		densityAtAgentPosition += Mathf.Abs(selfWeight)*Grid.instance.density[row, column];

		if (!((xNeighbour) < 0) & !((xNeighbour) > Grid.instance.cellsPerRow - 1)){	//As long as the cell exists
			densityAtAgentPosition += Mathf.Abs(neighbourXWeight)*Grid.instance.density[row, xNeighbour];
		}

		if (!((zNeighbour) < 0) & !((zNeighbour) > Grid.instance.cellsPerRow - 1)){			//As long as the cell exists
			densityAtAgentPosition += Mathf.Abs(neighbourZWeight)*Grid.instance.density[zNeighbour, column];
		}

		if (!((zNeighbour) < 0) & !((zNeighbour) > Grid.instance.cellsPerRow - 1) & !((xNeighbour) < 0) & !((xNeighbour) > Grid.instance.cellsPerRow - 1)){	//As long as the cell exists
			densityAtAgentPosition += Mathf.Abs(neighbourXZWeight)*Grid.instance.density[zNeighbour, xNeighbour];
		}
	}

	/**
	 * Calculate the continuum velocity caused by pressure from the grid
	 **/
	internal void calculateContinuumVelocity() {
		Vector3 tempContinuumVelocity = new Vector3(0,0,0);

		int xNeighbour = (int)(column + neighbourXWeight/Mathf.Abs(neighbourXWeight));	//Column for the neighbour which the agent contributes to
		int zNeighbour = (int)(row + neighbourZWeight/Mathf.Abs(neighbourZWeight));		//Row for the neighbour which the agent contributes to

		// Sides in current cell
		tempContinuumVelocity.x += selfLeftVelocityWeight*Grid.instance.cellMatrix[row, column].leftVelocityNode.velocity;

		tempContinuumVelocity.x += selfRightVelocityWeight*Grid.instance.cellMatrix[row, column].rightVelocityNode.velocity;

		tempContinuumVelocity.z += selfUpperVelocityWeight*Grid.instance.cellMatrix[row, column].upperVelocityNode.velocity;

		tempContinuumVelocity.z += selfLowerVelocityWeight*Grid.instance.cellMatrix[row, column].lowerVelocityNode.velocity;

		if (!((zNeighbour) < 0) & !((zNeighbour) > Grid.instance.cellsPerRow - 1)){	//As long as the cell exists
			tempContinuumVelocity.x += neighbourLeftVelocityWeight*Grid.instance.cellMatrix[zNeighbour, column].leftVelocityNode.velocity;
			tempContinuumVelocity.x += neighbourRightVelocityWeight*Grid.instance.cellMatrix[zNeighbour, column].rightVelocityNode.velocity;
		}

		if (!((xNeighbour) < 0) & !((xNeighbour) > Grid.instance.cellsPerRow - 1)){			//As long as the cell exists
			tempContinuumVelocity.z += neighbourUpperVelocityWeight*Grid.instance.cellMatrix[row, xNeighbour].upperVelocityNode.velocity;
			tempContinuumVelocity.z += neighbourLowerVelocityWeight*Grid.instance.cellMatrix[row, xNeighbour].lowerVelocityNode.velocity;
		}

		if (float.IsNaN(tempContinuumVelocity.x)){
			tempContinuumVelocity.Set (0, tempContinuumVelocity.y, tempContinuumVelocity.z);
		}

		if(float.IsNaN(continuumVelocity.z)){
			tempContinuumVelocity.Set (tempContinuumVelocity.x, tempContinuumVelocity.y, 0);
		}
		continuumVelocity = tempContinuumVelocity;
	}

	/**
	 * Move command (and all it includes) for this agent.
	 * Recalculate weights and contributions to grid after update.
	 **/
	internal void move(ref MapGen.map map) {
		changePosition (ref map);
		calculateRowAndColumn ();
		setWeights ();
		Grid.instance.cellMatrix[row, column].addVelocity(this);
		Grid.instance.cellMatrix[row, column].addDensity (this);
	}


	/**
	 * Set weight contributions to current cell radius. (Inverse bilinear interpolation)
	 **/
	public void setWeights(){
		float cellLength = Grid.instance.cellLength;
		float clSquared = Mathf.Pow (cellLength, 2);

		//An area the size of a cell is surrounded by each point.
		//AgentRelXPos: Side length of supposed area, outside current cell of agent - x direction
		//AgentRelZPos: Side length of supposed area, outside current cell of agent - z direction
		float sideOne = cellLength - Mathf.Abs(agentRelXPos); //Side length of supposed area of this agents position, x - direction
		float sideTwo = cellLength - Mathf.Abs(agentRelZPos); //Side length of supposed area of this agents position, z - direction

		// Weights on smaller areas inside and outside current cell
		//Area weight of neighboring cell in..
		neighbourXWeight = sideTwo*agentRelXPos/clSquared; // x direction
		neighbourZWeight = sideOne*agentRelZPos/clSquared; //z direction
		neighbourXZWeight = agentRelXPos*agentRelZPos/clSquared; //both x and z direction (diagonal from this agent's cell)

		//Own cell weight
		selfWeight = sideOne*sideTwo/clSquared; 


		//Now checking velocityNodes contribution
		//Offsets from each velocity node's center (also seen as a cell on each node)
		float rightShiftedRelXPos = cellLength / 2 + agentRelXPos;
		float leftShiftedRelXPos  = cellLength / 2 - agentRelXPos;
		float upperShiftedRelZPos = cellLength / 2 + agentRelZPos;
		float lowerShiftedRelZPos = cellLength / 2 - agentRelZPos;

		//Weight contributions to different velocityNodes (area / totalCellArea)
		selfRightVelocityWeight = rightShiftedRelXPos * sideTwo / clSquared;
		selfLeftVelocityWeight  = leftShiftedRelXPos  * sideTwo / clSquared;
		selfUpperVelocityWeight = upperShiftedRelZPos * sideOne / clSquared;
		selfLowerVelocityWeight = lowerShiftedRelZPos * sideOne / clSquared;

		neighbourRightVelocityWeight = rightShiftedRelXPos * Mathf.Abs(agentRelZPos) / clSquared;
		neighbourLeftVelocityWeight  = leftShiftedRelXPos  * Mathf.Abs(agentRelZPos) / clSquared;
		neighbourUpperVelocityWeight = upperShiftedRelZPos * Mathf.Abs(agentRelXPos) / clSquared;
		neighbourLowerVelocityWeight = lowerShiftedRelZPos * Mathf.Abs(agentRelXPos) / clSquared;
	}
}
