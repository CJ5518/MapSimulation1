using UnityEngine;

public class FpsSetter : MonoBehaviour {
	public int fps = 60;
	void Start() {
		Application.targetFrameRate = fps;
	}
}
