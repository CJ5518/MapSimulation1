using UnityEngine;
using UnityEngine.UI;

public class ColorWheel : MonoBehaviour {
	Camera uiCam;
	RectTransform wheel;
	float wheelHalfRes = 128f;
	
	public Image colorOutputImage;
	public Transform handle;

	public Image PrevColorIndicator;
	public Color PrevColor = Color.yellow;  //TEMPORARY

	void Start() {
		uiCam = transform.root.GetComponent<Canvas>().worldCamera;
		wheel = transform as RectTransform;
		UpdatePrevColor();
	}
	void Update() {
		colorOutputImage.color = ColorFromPostion(Input.mousePosition);

		//temp example thingy
		if (Input.GetMouseButtonUp(0)) SetPrevColor();
	}
	
	public void SetPrevColor() {
		PrevColor = ColorFromPostion(Input.mousePosition);
		UpdatePrevColor();
	}
	void UpdatePrevColor() {
		PrevColorIndicator.color = PrevColor;
		PrevColorIndicator.GetComponent<RectTransform>().anchoredPosition = PositionFromColor(PrevColor);
	}
	public Color ColorFromPostion(Vector3 pos) {
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			wheel, pos, uiCam, out Vector2 movePos);
		movePos = Vector2.ClampMagnitude(movePos, wheelHalfRes);

		handle.position = wheel.TransformPoint(movePos);

		float distance = movePos.magnitude / wheelHalfRes * 2;
		float angle = Vector2.SignedAngle(wheel.up, movePos) / 360f;
		if (angle < 0f) angle++;
		float sat = Mathf.Clamp01(distance);
		float val = Mathf.Clamp01(2 - distance);

		return Color.HSVToRGB(angle, sat, val);
	}
	public Vector3 PositionFromColor(Color col) {
		Color.RGBToHSV(col, out float h, out float s, out float v);
		float adjust = s < v ? s/2f : 1-v/2f;
		float distance = wheelHalfRes * adjust;
		Vector3 point = new Vector3(0, distance, 0);
		return RotatePointAroundPivot(point, wheel.forward, h * 360f);
	}
	public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angles) {
		return Quaternion.Euler(new Vector3(0,0,angles)) * (point - pivot) + pivot;
	}
}