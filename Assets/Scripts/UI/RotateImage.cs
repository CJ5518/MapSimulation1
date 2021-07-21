using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateImage : MonoBehaviour {

	public Vector3 deltaRot = Vector3.zero;
	Vector3 currentRot = Vector3.zero;
    //Hey Landon :D
	void Update() {
		currentRot += deltaRot;
		transform.localRotation = Quaternion.Euler(currentRot);
	}
}