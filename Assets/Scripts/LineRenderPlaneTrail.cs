using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderPlaneTrail : MonoBehaviour {

	LineRenderer line;

	public Transform start;
	public Transform end;
	
	Vector3[] positions = new Vector3[15];


	void Start() {
		line = GetComponent<LineRenderer>();
		line.positionCount = positions.Length+1;
		SetCurveSteep();
    }

	void SetCurve() {  // Cubic Bézier curve =D
		Vector3 lift = Vector3.forward * (10f + Vector3.Magnitude(end.position - start.position) / 5f);
		//Vector3 lift = Vector3.forward * Vector3.Magnitude(end.position - start.position); //fixed 45 start angle
		for (int i = 0; i < positions.Length; i++) {
			float distance = (float)i / positions.Length;
			Vector3 upLine = Vector3.Lerp(start.position, start.position + lift, distance);
			Vector3 downLine = Vector3.Lerp(end.position + lift, end.position, distance);
			Vector3 curve = Vector3.Lerp(upLine, downLine, distance);
			line.SetPosition(i, curve);
		}
		line.SetPosition(positions.Length, end.position);
	}

	void SetCurveSteep() { // higher-order curve  =o
		Vector3 travel = end.position - start.position;
		Vector3 lift = Vector3.forward * (10f + Vector3.Magnitude(travel)/5f);
		for (int i = 0; i < positions.Length; i++) {
			float distance = (float)i / positions.Length;
			Vector3 upLine = Vector3.Lerp(start.position, start.position + lift, distance);
			Vector3 hozLine = Vector3.Lerp(start.position + lift, end.position + lift, distance);
			Vector3 downLine = Vector3.Lerp(end.position + lift, end.position, distance);
			Vector3 upCurve = Vector3.Lerp(upLine, hozLine, distance);
			Vector3 downCurve = Vector3.Lerp(hozLine, downLine, distance);
			Vector3 curve = Vector3.Lerp(upCurve, downCurve, distance);
			line.SetPosition(i, curve);
		}
		line.SetPosition(positions.Length, end.position);
	}
}