using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDropScript : MonoBehaviour {
	public Image draggingImage;
	public Canvas parentCanvas;
	public void OnDragBegin() {
		draggingImage.gameObject.SetActive(true);

	}

	public void Drag() {
		Vector2 pos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, Input.mousePosition, parentCanvas.worldCamera, out pos);
		draggingImage.transform.position = parentCanvas.transform.TransformPoint(pos);
	}

	public void OnDragEnd() {
		draggingImage.gameObject.SetActive(false);
	}
}
