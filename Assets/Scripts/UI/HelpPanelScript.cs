using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpPanelScript : MonoBehaviour {
	public bool mouseOver = false;
	void Update() {
		Bounds bounds = Projection.GetRectTransformBounds(this.gameObject.GetComponent<RectTransform>());
		if (!bounds.Contains(Input.mousePosition)) {
			mouseOver = false;
		} else {
			mouseOver = true;
		}
	}
}