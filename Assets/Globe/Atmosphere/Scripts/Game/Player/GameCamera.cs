using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
	public event System.Action<Camera> gameCameraUpdateComplete;
	Camera cam;
	

	void Start() {
		cam = GetComponent<Camera>();
	}
	void LateUpdate() {
			gameCameraUpdateComplete?.Invoke(cam);
	}
	/*

	[System.Serializable]
	public struct ViewSettings
	{
		public Vector3 offset;
		public Vector3 lookTargetOffset;

		public ViewSettings(Vector3 offset, Vector3 lookTargetOffset)
		{
			this.offset = offset;
			this.lookTargetOffset = lookTargetOffset;
		}

		public static ViewSettings Lerp(ViewSettings a, ViewSettings b, float t)
		{
			Vector3 offset = Vector3.Lerp(a.offset, b.offset, t);
			Vector3 lookTargetOffset = Vector3.Lerp(a.lookTargetOffset, b.lookTargetOffset, t);
			return new ViewSettings(offset, lookTargetOffset);
		}
	}
	*/
}