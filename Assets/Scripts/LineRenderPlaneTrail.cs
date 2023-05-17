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
		SetCurveFancy();
    }

	void SetCurve() {  // Cubic Bézier curve =D   //(position * 1.2f moves out on sphere centered at origin)
		for (int i = 0; i < positions.Length; i++) {
			float distance = (float)i / positions.Length;
			Vector3 upLine = Vector3.Lerp(start.position, start.position * 1.2f, distance);
			Vector3 downLine = Vector3.Lerp(end.position * 1.2f, end.position, distance);
			Vector3 curve = Vector3.Lerp(upLine, downLine, distance);
			line.SetPosition(i, curve);
		}
		line.SetPosition(positions.Length, end.position);
	}

	void SetCurveFancy() { // higher-order curve  =o
		float SquareTravelDistance = Vector3.SqrMagnitude(end.position - start.position);  //usually between 0 and 7000 for this model
		float raiseProportion = (SquareTravelDistance + 50f)/ 70000f;
		Vector3 raisedStart = start.position + start.position * raiseProportion;
		Vector3 raisedEnd = end.position + end.position * raiseProportion;
		for (int i = 0; i < positions.Length; i++) {
			float progress = (float)i / positions.Length;
			Vector3 upLine = Vector3.Lerp(start.position, raisedStart, progress);
			Vector3 hozLine = Vector3.Lerp(raisedStart, raisedEnd, progress);
			Vector3 downLine = Vector3.Lerp(raisedEnd, end.position, progress);
			Vector3 upCurve = Vector3.Lerp(upLine, hozLine, progress);
			Vector3 downCurve = Vector3.Lerp(hozLine, downLine, progress);
			Vector3 curve = Vector3.Lerp(upCurve, downCurve, progress);
			line.SetPosition(i, curve);
		}
		line.SetPosition(positions.Length, end.position);
	}
}

	/*   //backup of old way of creating lift
	void SetCurveSteepOnPlane() {
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
	*/