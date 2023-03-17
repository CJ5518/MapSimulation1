using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPanelOpenClose : MonoBehaviour{

	public bool open;
	public bool horizontal;

	public GameObject horizontalButton;
	public GameObject verticalClosedButton;

	public float sizeOpen;
	public float sizeClosed;

	RectTransform panel;

	
	void Start(){
		panel = GetComponent<RectTransform>();
	}
	
	public void Toggle() {
		StartCoroutine(AnimateToggle());
	}

	private IEnumerator AnimateToggle() {
		float transitionPercent = 0f;
		RectTransform panel = GetComponent<RectTransform>();
		RectTransform.Axis axis = horizontal ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;

		while (transitionPercent < 1f) {
			float direction = open ? transitionPercent : 1 - transitionPercent;
			float size = Mathf.Lerp(sizeOpen, sizeClosed, direction);
			panel.SetSizeWithCurrentAnchors(axis, size);
			transitionPercent += 5 * Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		panel.SetSizeWithCurrentAnchors(axis, open? sizeClosed: sizeOpen);

		open = !open;
		if (horizontal) {
			horizontalButton.SetActive(open);
			verticalClosedButton.SetActive(!open);
		}
	}
}