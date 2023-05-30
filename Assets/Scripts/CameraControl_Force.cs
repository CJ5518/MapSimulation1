using UnityEngine;

public class CameraControl_Force : MonoBehaviour {
	
	Transform cam;
	float zoomInput = 0f;
	Vector2 rotationInput = Vector2.zero;
	Vector2 zoomRange = new Vector2(-155f, -250f);
	Vector2 zoomSpeedRange = new Vector2(0.1f, 1f);
	float zoomPercent = 0.442f;
	float zoom = -280f;
	float zoomDamping = 5f;
	Vector2 zoomAngleRange = new Vector2(-50f, 0f);
	float zoomAngleCurveSharpness = 5f;
	Vector2 rotationSpeedRange = new Vector2(200f,5000f);
	Vector2 lastMousePos;
	Vector4 bounds = new Vector4(24f,45f,163f,212f); //xmin,xmax,ymin,ymax

	void Start() {
		cam = Camera.main.transform;
	}
	void FixedUpdate() {
		UpdateInputs();
		Zoom();
		Rotation();
		ClampRotation();
	}

	void UpdateInputs() {
		Vector2 mouseDelta = lastMousePos - (Vector2)Input.mousePosition;
		lastMousePos = Input.mousePosition;
		
		zoomInput = Input.mouseScrollDelta.y * 20f;
		zoomInput += Input.GetMouseButton(2) ? mouseDelta.y / 5f : 0;
		float KeyZoom = Input.GetKey(KeyCode.Z) ? -1f : Input.GetKey(KeyCode.X) ? 1f : 0f;
		zoomInput += KeyZoom;
		rotationInput = Input.GetMouseButton(1) ? mouseDelta : Vector2.zero;
	}
	void Zoom() {
		float zoomSpeed = Mathf.Lerp(zoomSpeedRange.x, zoomSpeedRange.y, zoomPercent);
		zoomPercent = Mathf.Clamp01(zoomPercent + zoomInput * zoomSpeed * Time.deltaTime);
		zoom = Mathf.Lerp(zoom, Mathf.Lerp(zoomRange.x, zoomRange.y, zoomPercent), zoomDamping * Time.deltaTime);
		cam.localPosition = new Vector3(0, 0, zoom);
		ZoomRotation();
	}
	void ZoomRotation() {
		float currentZoomPercent = -(cam.localPosition.z - zoomRange.x) / (zoomRange.x - zoomRange.y);
		float anglePercent = (-1 / (1 + zoomAngleCurveSharpness * currentZoomPercent) + 1) * (zoomAngleCurveSharpness + 1) / zoomAngleCurveSharpness;
		float angle = Mathf.Lerp(zoomAngleRange.x, zoomAngleRange.y, anglePercent);
		cam.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}
	void Rotation() {
		float rotInputX = rotationInput.x;
		float rotInputY = rotationInput.y;
		if (rotInputX == 0 && rotInputY == 0) return;

		float rotationSpeed = Mathf.Lerp(rotationSpeedRange.x, rotationSpeedRange.y, zoomPercent);

		//raw rotation - rotation relative to camera controller (this) based on input;
		Vector3 rot = (rotInputY * transform.right - rotInputX * transform.up);
		//rotation correction - move towards the plane of rotation;
		Vector3 idealForward = Vector3.ProjectOnPlane(transform.forward, rot);
		rot += Vector3.Cross(transform.forward, idealForward);
		rot *= rotationSpeed;
		GetComponent<Rigidbody>().AddTorque(rot);
	}
	void NorthUp() {
		/************************************ smooth
		Vector3 rot = Vector3.zero;
		Vector3 idealNorth = Vector3.ProjectOnPlane(Vector3.up, transform.forward);
		rot += Vector3.Cross(transform.up, idealNorth);
		rot *= 30000;
		GetComponent<Rigidbody>().AddTorque(rot); */
	}
	void ClampRotation() {
		Vector3 rot = transform.rotation.eulerAngles;
		rot.x = Mathf.Clamp(rot.x, bounds.x, bounds.y);
		rot.y = Mathf.Clamp(rot.y, bounds.z, bounds.w);
		transform.rotation = Quaternion.Euler(rot.x, rot.y, 0f);
	}
}