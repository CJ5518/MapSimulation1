using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlLandon : MonoBehaviour {

	Vector2 zoomRange = new Vector2(20f, 400f);
	Vector2 angleRange = new Vector2(-50f, 0f);
	Vector4 panBoundsRange = new Vector4(80f, 70f, 300f, 200f); // near x, near y, far x, far y

	Transform cam;
	float distancePercent = 0.3f;
	float rawDistancePercent = 0.3f;
	Vector3 rawCamPosition;

	bool initializedTwoFingerZoom = false;
	float fingersDistance = 0f;
	bool initializedTwoFingerPan = false;
	Vector2 fingersPos = Vector2.zero;


	void Start() {
		cam = Camera.main.transform;
		rawCamPosition = cam.position;
		UpdateCameraPosition();
		UpdateCameraRotation();
	}
	void Update() {
		rawDistancePercent = Mathf.Clamp01(rawDistancePercent + Input.mouseScrollDelta.y * (rawDistancePercent + 0.05f) / 10f);
		rawDistancePercent = Mathf.Clamp01(rawDistancePercent + TwoFingerZoomInput() * (rawDistancePercent + 0.05f) / 100f);
		distancePercent = (rawDistancePercent + 5f * distancePercent) / 6f;

		//DebugDrawBounds();
		UpdateCameraRotation();
		UpdateCameraPosition();
	}
	

	float TwoFingerZoomInput() {
		if (Input.touchCount < 2) {
			initializedTwoFingerZoom = false;
			return 0f;
		}
		float distance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
		if (initializedTwoFingerZoom) {
			float deltaDistance = fingersDistance - distance;
			fingersDistance = distance;
			return deltaDistance;
		}
		else {
			fingersDistance = distance;
			initializedTwoFingerZoom = true;
			return 0f;
		}
	}
	Vector2 TwoFingerPanInput() {
		if (Input.touchCount < 2) {
			initializedTwoFingerPan = false;
			return Vector2.zero;
		}
		Vector2 pos = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2f;
		if (initializedTwoFingerPan) {
			Vector2 deltaPos = pos - fingersPos;
			fingersPos = pos;
			deltaPos.y *= -1;
			return deltaPos;
		}
		else {
			fingersPos = pos;
			initializedTwoFingerPan = true;
			return Vector2.zero;
		}
	}


	void UpdateCameraPosition () {
		if (Input.GetMouseButton(1)) {
			float MouseX = Input.GetAxis("Mouse X") * 6f * (0.1f + distancePercent);
			float MouseY = -Input.GetAxis("Mouse Y") * 6f * (0.1f + distancePercent);
			rawCamPosition += new Vector3(MouseX, MouseY, 0f);
		}
		rawCamPosition += (Vector3)TwoFingerPanInput() / 6f;
		rawCamPosition = ApplyCameraBounds(rawCamPosition);
		cam.position = (rawCamPosition + 5f * cam.position) / 6f;
	}

	Vector3 ApplyCameraBounds(Vector3 pos) {
		float panBoundX = Mathf.Lerp(panBoundsRange.x, panBoundsRange.z, distancePercent);
		float panBoundY = Mathf.Lerp(panBoundsRange.y, panBoundsRange.w, distancePercent);
		float posX = Mathf.Clamp(pos.x, -panBoundX, panBoundX);
		float posY = Mathf.Clamp(pos.y, -panBoundY, panBoundY);
		float posZ = Mathf.Lerp(zoomRange.x, zoomRange.y, distancePercent);
		return new Vector3(posX, posY, posZ);
	}

	void UpdateCameraRotation() {
		float curve = 12f; // higher number -> tighter curve
		float anglePercent = (-1 / (1 + curve * distancePercent) + 1) * (curve + 1) / curve;

		float angle = Mathf.Lerp(angleRange.x, angleRange.y, anglePercent);
		cam.rotation = Quaternion.Euler(angle, 180f, 0f);
	}

	void DebugDrawBounds() {
		Vector3 nearUpperLeft = new Vector3(-panBoundsRange.x, panBoundsRange.y, zoomRange.x);
		Vector3 nearUpperRight = new Vector3(panBoundsRange.x, panBoundsRange.y, zoomRange.x);
		Vector3 nearLowerLeft = new Vector3(-panBoundsRange.x, -panBoundsRange.y, zoomRange.x);
		Vector3 nearLowerRight = new Vector3(panBoundsRange.x, -panBoundsRange.y, zoomRange.x);
		Vector3 farUpperLeft = new Vector3(-panBoundsRange.z, panBoundsRange.w, zoomRange.y);
		Vector3 farUpperRight = new Vector3(panBoundsRange.z, panBoundsRange.w, zoomRange.y);
		Vector3 farLowerLeft = new Vector3(-panBoundsRange.z, -panBoundsRange.w, zoomRange.y);
		Vector3 farLowerRight = new Vector3(panBoundsRange.z, -panBoundsRange.w, zoomRange.y);

		Debug.DrawLine(nearUpperLeft, farUpperLeft, Color.green);
		Debug.DrawLine(nearUpperRight, farUpperRight, Color.green);
		Debug.DrawLine(nearLowerLeft, farLowerLeft, Color.green);
		Debug.DrawLine(nearLowerRight, farLowerRight, Color.green);
		Debug.DrawLine(nearUpperLeft, nearUpperRight, Color.yellow);
		Debug.DrawLine(nearUpperLeft, nearLowerLeft, Color.yellow);
		Debug.DrawLine(nearUpperRight, nearLowerRight, Color.yellow);
		Debug.DrawLine(nearLowerLeft, nearLowerRight, Color.yellow);
		Debug.DrawLine(farUpperLeft, farUpperRight, Color.cyan);
		Debug.DrawLine(farUpperLeft, farLowerLeft, Color.cyan);
		Debug.DrawLine(farUpperRight, farLowerRight, Color.cyan);
		Debug.DrawLine(farLowerLeft, farLowerRight, Color.cyan);
	}

}