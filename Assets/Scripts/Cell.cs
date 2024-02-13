﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour {

	internal int row, column;
	internal float cellLength;

	internal float currentDensity;

	internal VelocityNode leftVelocityNode;
	internal VelocityNode rightVelocityNode;
	internal VelocityNode upperVelocityNode;
	internal VelocityNode lowerVelocityNode;
	internal float availableArea = 1.0f;
	internal Renderer renderer;

	/**
	 * Initialize this cell
	 **/ 
	public void setProperties(int row, int column, float cellLength) {
		this.row = row; this.column = column; this.cellLength = cellLength;
		renderer = GetComponent<Renderer> ();
	}

	/**
	 * Calculate the available area of this cell (Area not occupied by colliders)
	 **/ 
	internal void calculateAvailableArea() {
		int counter = 0;
		Vector3 start = transform.position;
		start.x -= Grid.instance.cellLength / 2;
		start.z -= Grid.instance.cellLength / 2;
		start.y = 0f;
		for (int i = 0; i < Grid.instance.cellLength; ++i) {
			for (int j = 0; j < Grid.instance.cellLength; ++j) {
				if (Physics.Raycast (new Vector3 (start.x, 100, start.z), new Vector3 (0, -10, 0), 150f)) {
					counter += 1;
				}
				start.x += 1;
			}
			start.x = transform.position.x - Grid.instance.cellLength / 2;
			start.z += 1;
		}

		availableArea =  1 - counter/Mathf.Pow(Grid.instance.cellLength, 2);
		if (availableArea < 0.03f) {
			availableArea = 0.001f;
		}

	}

	/**
	 * Create or assign the velocitynodes for this cell
	 **/ 
	internal void setVelocityNodes(){
		if (Grid.instance.xEdgeVelocityNodeMatrix [row, column] == null) {
			leftVelocityNode = Instantiate(Grid.instance.velocityNodePrefab) as VelocityNode;
			leftVelocityNode.init (row, column, true);
			leftVelocityNode.transform.position = transform.position + new Vector3 (-cellLength / 2, 0, 0);
			Grid.instance.xEdgeVelocityNodeMatrix [row, column] = leftVelocityNode;
			leftVelocityNode.transform.parent = transform;
		} else {
			leftVelocityNode = Grid.instance.xEdgeVelocityNodeMatrix [row, column]; 
		}
			
		if (Grid.instance.xEdgeVelocityNodeMatrix [row, column+1] == null) {
			rightVelocityNode = Instantiate(Grid.instance.velocityNodePrefab) as VelocityNode;
			rightVelocityNode.init (row, column + 1, true);
			rightVelocityNode.transform.position = transform.position + new Vector3 (cellLength / 2, 0, 0);
			Grid.instance.xEdgeVelocityNodeMatrix [row, column+1] = rightVelocityNode;
			rightVelocityNode.transform.parent = transform;
		} else {
			rightVelocityNode = Grid.instance.xEdgeVelocityNodeMatrix [row, column+1]; 
		}

		if (Grid.instance.zEdgeVelocityNodeMatrix [row+1, column] == null) {
			upperVelocityNode = Instantiate(Grid.instance.velocityNodePrefab) as VelocityNode;
			upperVelocityNode.init (row+1, column, false);
			upperVelocityNode.transform.position = transform.position + new Vector3 (0, 0, cellLength / 2);
			Grid.instance.zEdgeVelocityNodeMatrix [row+1, column] = upperVelocityNode;
			upperVelocityNode.transform.parent = transform;
		} else {
			upperVelocityNode = Grid.instance.zEdgeVelocityNodeMatrix [row+1, column]; 
		}

		if (Grid.instance.zEdgeVelocityNodeMatrix [row, column] == null) {
			lowerVelocityNode = Instantiate(Grid.instance.velocityNodePrefab) as VelocityNode;
			lowerVelocityNode.init (row, column, false);
			lowerVelocityNode.transform.position = transform.position + new Vector3 (0, 0, -cellLength / 2);
			Grid.instance.zEdgeVelocityNodeMatrix [row, column] = lowerVelocityNode;
			lowerVelocityNode.transform.parent = transform;
		} else {
			lowerVelocityNode = Grid.instance.zEdgeVelocityNodeMatrix [row, column]; 
		}
	}
		
	/**
	 * Convert the density accumelated by this cell to a continous form
	 **/ 
	internal void splatDensity() {
		Grid.instance.density[row, column] = currentDensity / (float)Mathf.Pow(cellLength, 2);
		currentDensity = 0;
	}

	/**
	 * Calculate the mean velocity of this cell in both directionsa from the velocitynodes
	 **/ 
	internal void setMeanVelocity(){
		Grid.instance.xMeanVelocity [row, column] = (leftVelocityNode.velocity + rightVelocityNode.velocity) / 2.0f;
		Grid.instance.zMeanVelocity [row, column] = (upperVelocityNode.velocity + lowerVelocityNode.velocity) / 2.0f;
	}

	/**
	 * Have an agent add their eulerian density contribution to this cell
	 **/ 
	internal void addDensity(Agent agent){
		int xNeighbour = (int)(column + agent.neighbourXWeight/Mathf.Abs(agent.neighbourXWeight));	//Column for the neighbour which the agent contributes to
		int zNeighbour = (int)(row + agent.neighbourZWeight/Mathf.Abs(agent.neighbourZWeight));		//Row for the neighbour which the agent contributes to 

		currentDensity += Mathf.Abs(agent.selfWeight);

		if (xNeighbour >= 0 && xNeighbour < Grid.instance.cellsPerRow) {
			Grid.instance.cellMatrix[row, xNeighbour].currentDensity += Mathf.Abs(agent.neighbourXWeight);
		}
		if (zNeighbour >= 0 && zNeighbour < Grid.instance.cellsPerRow) {
			Grid.instance.cellMatrix[zNeighbour, column].currentDensity += Mathf.Abs(agent.neighbourZWeight);
		}
		if (xNeighbour >= 0 && xNeighbour < Grid.instance.cellsPerRow && zNeighbour >= 0 && zNeighbour < Grid.instance.cellsPerRow) {
			Grid.instance.cellMatrix[zNeighbour, xNeighbour].currentDensity += Mathf.Abs(agent.neighbourXZWeight);
		}
	}

	/**
	 * Have an agent add their density and velocity to this cells velocitynodes, as well as neighbouring ones
	 **/ 
	internal void addVelocity(Agent agent){
		int xNeighbour = (int)(column + agent.neighbourXWeight/Mathf.Abs(agent.neighbourXWeight));	//Column for the neighbour which the agent contributes to
		int zNeighbour = (int)(row + agent.neighbourZWeight/Mathf.Abs(agent.neighbourZWeight));		//Row for the neighbour which the agent contributes to
		Vector3 vel = agent.velocity;
		// Sides in current cell
		leftVelocityNode.tempVelocity += vel * agent.selfLeftVelocityWeight;
		leftVelocityNode.weights += agent.selfLeftVelocityWeight;

		rightVelocityNode.tempVelocity += vel * agent.selfRightVelocityWeight;
		rightVelocityNode.weights += agent.selfRightVelocityWeight;

		upperVelocityNode.tempVelocity += vel*agent.selfUpperVelocityWeight;
		upperVelocityNode.weights += agent.selfUpperVelocityWeight;

		lowerVelocityNode.tempVelocity += vel*agent.selfLowerVelocityWeight;
		lowerVelocityNode.weights += agent.selfLowerVelocityWeight;

		if (!((zNeighbour) < 0) & !((zNeighbour) > Grid.instance.cellsPerRow - 1)){	//As long as the cell exists
			Grid.instance.cellMatrix[zNeighbour, column].leftVelocityNode.tempVelocity += vel*agent.neighbourLeftVelocityWeight;
			Grid.instance.cellMatrix[zNeighbour, column].leftVelocityNode.weights += agent.neighbourLeftVelocityWeight;

			Grid.instance.cellMatrix[zNeighbour, column].rightVelocityNode.tempVelocity += vel*agent.neighbourRightVelocityWeight;
			Grid.instance.cellMatrix[zNeighbour, column].rightVelocityNode.weights += agent.neighbourRightVelocityWeight;
		}

		if (!((xNeighbour) < 0) & !((xNeighbour) > Grid.instance.cellsPerRow - 1)){	//As long as the cell exists
			Grid.instance.cellMatrix[row, xNeighbour].upperVelocityNode.tempVelocity += vel*agent.neighbourUpperVelocityWeight;
			Grid.instance.cellMatrix[row, xNeighbour].upperVelocityNode.weights += agent.neighbourUpperVelocityWeight;

			Grid.instance.cellMatrix[row, xNeighbour].lowerVelocityNode.tempVelocity += vel*agent.neighbourLowerVelocityWeight;
			Grid.instance.cellMatrix[row, xNeighbour].lowerVelocityNode.weights += agent.neighbourLowerVelocityWeight;
		}
	}

	/**
	 * Illustrate velocity on this cell
	 **/ 
	internal void drawVelocityField() {
		Vector3 v1;
		Vector3 v2;
		Vector3 u1;
		Vector3 u2;
		float vleft = Grid.instance.xEdgeVelocity [leftVelocityNode.cellRow, leftVelocityNode.cellCol],
			 vRight = Grid.instance.xEdgeVelocity [rightVelocityNode.cellRow, rightVelocityNode.cellCol],
		        vUp = Grid.instance.zEdgeVelocity [upperVelocityNode.cellRow, upperVelocityNode.cellCol],
		      vDown = Grid.instance.zEdgeVelocity [lowerVelocityNode.cellRow, lowerVelocityNode.cellCol];

		Vector3 velocity = new Vector3 ((vleft + vRight) / 2, 0f, (vUp + vDown) / 2);

		velocity = velocity.normalized*4f;

		Vector3 position = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
		v1 = position+velocity;
		v2 = position-velocity;
		u1 = Quaternion.Euler(0, 45, 0) * (-velocity.normalized) + v1;
		u2 = Quaternion.Euler(0, -45, 0) * (-velocity.normalized) + v1;

		//Draws
		Debug.DrawLine (position, v1, Color.grey);
		Debug.DrawLine (position, v2, Color.grey);
		Debug.DrawLine (v1, u1, Color.grey);
		Debug.DrawLine (v1, u2, Color.grey);
	}

	/**
	 * Set the color for this cell (when visualizing density)
	 **/
	public void setColor() {
		float n = Grid.instance.density [row, column] / (availableArea*Grid.maxDensity);
		if (n > 1) {
			n = 1; //Constrain to maxdensity.
		}

		byte m = (byte)(25 + Mathf.Abs(n) * (255 - 25)); //Todo: Make better
		this.GetComponent<Renderer>().material.color = new Color32 (m, m, m, 1);
	}
}
