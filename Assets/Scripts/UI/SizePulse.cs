using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizePulse : MonoBehaviour {


	public bool pulse = true;
	bool endPulsing = false;
	float pulseProgress = 0.5f;

	void Update() {
		if (pulse) {
			if (endPulsing) endPulsing = false;
			pulseProgress = (Mathf.Sin(2 * Time.time)+ Mathf.Cos(6 * Time.time)) / 4 + 0.5f;
			transform.localScale = Vector3.Lerp(Vector3.one * 1.1f, Vector3.one * 0.7f, pulseProgress);
		}
		else if (!endPulsing) {
			transform.localScale = Vector3.one * 0.9f;
			endPulsing = true;
		}
	}
	
}