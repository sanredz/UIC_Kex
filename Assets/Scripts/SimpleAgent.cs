using UnityEngine;
using System.Collections;

public class SimpleAgent : MonoBehaviour {
	// OLD AGENT CLASS
//	public int gridPosX, gridPosZ;
//	public Vector3 velocity;
//	internal float agentRelXPos, agentRelZPos;
//	internal float neighbourXWeight, neighbourZWeight, neighbourXZWeight, selfWeight;
//
//	internal void move() {
//		//Which cell am i in currently?
//		int row = (int)Mathf.Floor((transform.position.z - Main.zMinMax.x)/Grid.instance.cellLength); 
//		int column = (int)Mathf.Floor((transform.position.x - Main.xMinMax.x)/Grid.instance.cellLength); 
//		if (row < 0)
//			row = 0; 
//		if (column < 0)
//			column = 0;
//		if (row > Grid.instance.cellsPerRow - 1) {
//			row = Grid.instance.cellsPerRow - 1;
//		}
//		if (column > Grid.instance.cellsPerRow - 1) {
//			column = Grid.instance.cellsPerRow - 1;
//		}
//		agentRelXPos = transform.position.x - Grid.instance.cellMatrix [row, column].transform.position.x;
//		agentRelZPos = transform.position.z - Grid.instance.cellMatrix [row, column].transform.position.z;
//
//		setWeights ();
//		Grid.instance.cellMatrix[row, column].addDensity (this);
//	}
//
//	public void setWeights(){
//		float cellLength = Grid.instance.cellLength;
//		float clSquared = Mathf.Pow (cellLength, 2);
//		// To minimize number of calculations, give better names later
//		float tempConstant1 = cellLength - Mathf.Abs(agentRelZPos);
//		float tempConstant2 = cellLength - Mathf.Abs(agentRelXPos); 
//
//		// Weights on different areas of intersecting areas.. 
//		neighbourXWeight = tempConstant1*agentRelXPos/clSquared;
//	//	Debug.Log ("Calculated: " + neighbourXWeight + " t1: " + tempConstant1 + " relXPos " + agentRelXPos + " clSquared: " + clSquared);
//		neighbourZWeight = tempConstant2*agentRelZPos/clSquared;
//		neighbourXZWeight = agentRelXPos*agentRelZPos/clSquared;
//		selfWeight = tempConstant2*tempConstant1/clSquared;
//	
//	}
}
