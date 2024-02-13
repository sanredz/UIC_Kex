using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;
using System;

//NEW
using System.Diagnostics;
using System.IO;
//

public class lcpsolverold {


	private SparseMatrix A;
	private DenseVector b;
	private DenseVector x;
	private DenseVector l;

	//Constant
	private double gammaConstant;

	//Steplength
	private double normA; 
	private double alphaBar;

	//Error constant
	private double epsilon;

	internal double[] initialize(double[] bArray, double[] xArray, double[] lArray, ref SparseMatrix AA) {
		b = DenseVector.OfArray(bArray);
		x = DenseVector.OfArray(xArray);
		l = DenseVector.OfArray(lArray);
		A = AA; //SparseMatrix.OfArray(matrixArray);
		gammaConstant = 1;
		normA = A.FrobeniusNorm(); //Maybe take the infinity norm, try different approaches
		alphaBar = 1/(2*normA);
		epsilon = 0.00000001*b.L2Norm();
		//	UnityEngine.Debug.Log(alphaBar);
		DenseVector r = (DenseVector)(A*x + b); //		DenseVector r = (DenseVector)(A*x - b); //[Dostal]
		DenseVector p = phi(x);
		double alphaCG;
		//	A [0, 6] = 0;
		int cnt = 0;
		while (v(x).L2Norm() > epsilon) {
			cnt += 1;

			double normB = B(x).L2Norm();
			if ((normB*normB) <= gammaConstant*phiTilde(x).DotProduct(phi(x))) {
				//1. Trial conjugate gradient step
				DenseVector Ap = (DenseVector)(A*p);
				alphaCG = (r.DotProduct(p))/p.DotProduct(Ap);
				DenseVector y = (DenseVector)(x - p.Multiply(alphaCG));
				double alphaF = calcAlphaF(p);
				if (alphaCG <= alphaF) {
					//2. Conjugate gradient step;
					x = y;
					r = (DenseVector)(r - (Ap).Multiply(alphaCG));
					double gamma = (phi(y).DotProduct(Ap))/(p.DotProduct(Ap));
					p = (DenseVector)(phi(y) - p.Multiply(gamma));
				} else {
					//3. Expansion step
					x = (DenseVector)(x - p.Multiply(alphaF));
					r = (DenseVector)(r - (Ap).Multiply(alphaF));
					x = projection((DenseVector)(x - phi(x).Multiply(alphaBar)));
					r = (DenseVector)(A*x + b); //					r = (DenseVector)(A*x - b); //[Dostal]
					p = phi(x);
				}
			} else {
				//4. Proportioning step
				DenseVector d = B(x);
				DenseVector Ad = (DenseVector)(A*d);
				if (d.L2Norm() != 0){
					alphaCG = (r.DotProduct(d))/(d.DotProduct(Ad));
				} else {
					alphaCG = (r.DotProduct(d))/(d.DotProduct(Ad));
				}
				x = (DenseVector)(x - d.Multiply(alphaCG));
				r = (DenseVector)(r - (Ad).Multiply(alphaCG));
				p = phi(x);
			}
		}
		//UnityEngine.Debug.Log ("Took: " + cnt);
		return x.ToArray ();
	}

	private DenseVector g(DenseVector vector) {
		return (DenseVector)(A*vector + b); //		return A*x - b; //[Dostal]
	}

	private DenseVector phi(DenseVector vector) {
		DenseVector tempPhi = DenseVector.Create(vector.Count, 0);
		DenseVector G = g(vector);
		for (int i = 0; i < x.Count; i++) {
			if (vector.At(i).Equals(0)) {
				tempPhi.At(i, 0);
			} else {
				tempPhi.At(i,G.At(i));
			}
		}
		return tempPhi;
	}

	private DenseVector B(DenseVector vector) {
		DenseVector tempB = DenseVector.Create(vector.Count, 0);
		DenseVector G = g(vector);
		for (int i = 0; i < vector.Count; i++) {
			if ((vector.At(i).Equals(l.At(i))) && (G.At(i) < 0)) {
				tempB.At(i, G.At(i));
			} else {
				tempB.At(i, 0);
			}
		}
		return tempB;
	}

	private DenseVector v(DenseVector vector) {
		return phi(vector) + B(vector);
	}

	private DenseVector phiTilde(DenseVector vector) {
		DenseVector tempPhiTilde = DenseVector.Create(vector.Count, 0);
		DenseVector Phi = phi(vector);
		for (int i = 0; i < vector.Count; i++) {
			double temp = (vector.At(i) - l.At(i))/alphaBar;
			if (temp < Phi.At(i)) {
				tempPhiTilde.At(i, temp);
			} else {
				tempPhiTilde.At(i, Phi.At(i));
			}
		}
		return tempPhiTilde;
	}

	private DenseVector projection(DenseVector vector) {
		DenseVector xl = vector - l;
		for (int i = 0; i < xl.Count; i++) {
			if (xl.At(i) < 0) {
				xl.At(i, 0);
			}
		}	
		return l+xl;
	}

	private double calcAlphaF(DenseVector p) {
		double alphaF = (double)(Mathf.Infinity);
		for (int i = 0; i < x.Count; i++) {
			if (p.At(i) > 0) {
				double temp = (x.At(i) - l.At(i)) / p.At(i);
				if (temp < alphaF) {
					alphaF = temp;
				}
			}
		}
		return alphaF;
	}

}