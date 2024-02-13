using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System;


public class LCPSolverProj : LCPSolver {

	private double epsilon;


	internal bool checkEndCondition() {
		double[] z = PlusMinusVec (TwoMulOne (x), b, true);
		//z >= 0
		bool conditionA = true;
		//x >= 0
		bool conditionB = true;
		//zTx = 0
		bool conditionC = false;
		double sum = 0.0;

		for (int i = 0; i < z.GetLength (0); ++i) {
			if (z [i] < 0)
				conditionA = false;
			if (x [i] < 0)
				conditionB = false;
			sum += z [i] * x [i];
		}
		if (Math.Abs (sum) < epsilon) {
			conditionC = true;
		}
		return conditionA && conditionB && conditionC;
	}
	internal double[] LCPSolve(List<List<denseMatrixNode>> aList, double[,] aMatrix, double[] bArray, double[] xArray, double[] lArray) {
		this.A = aList;
		this.b = bArray;
		this.x = xArray;
		this.l = lArray;
		epsilon = Grid.instance.solverEpsilon;

		int maxiter = Grid.instance.solverMaxIterations, cnt = 0;
		double delta = 1.3; 
		for (int k = 0; k < maxiter; ++k) {
			cnt += 1;
			if (checkEndCondition ()) {
			//	UnityEngine.Debug.Log ("Break due to checkend");
				break;
			}
			double oldXMax = 0;
			double newXMax = 0;
			for (int i = 0; i < b.GetLength (0); ++i) {
				oldXMax = Math.Max (oldXMax, x [i]);
				if (Math.Abs (aMatrix[i, i]) > epsilon) {
					x[i] = Math.Max (0.0, x [i] - delta * (OneMultOne (i, x) + b[i]) / aMatrix [i, i]);
				}
				newXMax = Math.Max (newXMax, x [i]);
			}
			//If there is no real change, exit
			if (Math.Abs (oldXMax - newXMax) < epsilon) {
		//		UnityEngine.Debug.Log ("Break due to no diff");
				break;
			}
				
		}
		if (cnt == maxiter)
			UnityEngine.Debug.Log ("Used full iterations");
	//	UnityEngine.Debug.Log ("Took: " + cnt + " iterations");
		return x;
	}
}
