using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderPlaneTrail : MonoBehaviour {

	LineRenderer line;

	public Vector3 start;
	public Vector3 end;
	//Lifetime is used here for animation
	//Actual life and death is covered in SimulationAirports
	public float lifetime;
	public float startDelay;
	private float lifeStart = 0.0f;
	
	Vector3[] positions = new Vector3[15];

	void Start() {
		line = GetComponent<LineRenderer>();
		line.positionCount = positions.Length+1;
		var tmp = start;
		start = end;
		end = tmp;
		SetCurve();
		StartCoroutine("doAnimation");
    }

	IEnumerator doAnimation() {
		yield return new WaitForSeconds(startDelay);
		lifeStart = Time.realtimeSinceStartup;
		while (true) {
			Material material = GetComponent<LineRenderer>().material;
			//animation goes from -.5 to 1
			material.SetTextureOffset("_MainTex", new Vector2(Mathf.Lerp(-0.5f, 1.0f, (Time.realtimeSinceStartup - lifeStart) / lifetime), material.GetTextureOffset("_MainTex").y));
			yield return null;
		}
	}

	void SetCurve() { // Cubic Bézier curve =D
		float SquareTravelDistance = Vector3.SqrMagnitude(end - start);  //usually between 0 and 7000 for this model
		//What do these numbers mean?
		float raiseProportion = (SquareTravelDistance + 2000f)/ 70000f;
		Vector3 raisedStart = start + start * raiseProportion;
		Vector3 raisedEnd = end + end * raiseProportion;
		for (int i = 0; i < positions.Length; i++) {
			float progress = (float)i / positions.Length;
			Vector3 upLine = Vector3.Lerp(start, raisedStart, progress);
			Vector3 hozLine = Vector3.Lerp(raisedStart, raisedEnd, progress);
			Vector3 downLine = Vector3.Lerp(raisedEnd, end, progress);
			Vector3 upCurve = Vector3.Lerp(upLine, hozLine, progress);
			Vector3 downCurve = Vector3.Lerp(hozLine, downLine, progress);
			Vector3 curve = Vector3.Lerp(upCurve, downCurve, progress);
			line.SetPosition(i, curve);
		}
		line.SetPosition(positions.Length, end);
	}
}