using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projection {
	public static float projectLongitude(float x) {
		return (Screen.width / 360.0f) * (180.0f + x);
	}
}