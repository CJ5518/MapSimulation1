//By Carson Rueber

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Draggable button
internal class DraggableButton : Button {
	//Variables
	private Vector3 offset;
	private bool dragging;

	//Set dragging to be true, and offset accordingly
	public override void OnPointerDown(PointerEventData eventData) {
		base.OnPointerDown(eventData);
		offset = Input.mousePosition - GetComponent<RectTransform>().position;
		dragging = true;
	}

	//Set dragging to be false
	public override void OnPointerUp(PointerEventData eventData) {
		base.OnPointerUp(eventData);
		dragging = false;
	}

	//Move the button if we're dragging it
	public void Update() {
		if (!dragging)
			return;
		GetComponent<RectTransform>().position = Input.mousePosition - offset;
	}
}
