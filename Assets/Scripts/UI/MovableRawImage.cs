//By Carson Rueber

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//A RawImage that can be panned and zoomed
public class MovableRawImage : RawImage {
	private Vector3 offset;
	private bool dragging;

	//Drag start
	public void DragStart() {
		offset = Input.mousePosition - rectTransform.position;
		dragging = true;
	}

	//Drag end
	public void DragEnd() {
		dragging = false;
	}

	//Zoom
	public void OnScroll() {
		//Scale factor
		float y = Input.mouseScrollDelta.y;
		float factor = (0.05f * y) + 1.0f;

		Vector2 worldPosBefore, worldPosAfter;

		//Scale
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out worldPosBefore);
		rectTransform.localScale = rectTransform.localScale * factor;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out worldPosAfter);

		//Move according to the mosue position, keeping it at the same worldPosition as it was before
		Vector2 diff = worldPosAfter - worldPosBefore;
		rectTransform.position = rectTransform.position + (new Vector3(diff.x, diff.y) * rectTransform.localScale.x);
	}

	//Get a pixel on the texture from a screen coordinate
	public Vector2 getPixelFromScreenCoord(Vector2 coord) {
		//Convert it to be within the bounds of the image
		Vector2 ret = getLocalPositionInRectangle(coord);
		//Scale accordingly
		ret.x /= rectTransform.rect.width;
		ret.y /= rectTransform.rect.height;
		ret.x *= texture.width;
		ret.y *= texture.height;
		return ret;
	}

	//FIX
	//Doesn't work with scaling at the moment, maybe?
	public Vector2 getLocalPositionInRectangle(Vector2 screenCoord) {
		Vector2 ret;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenCoord, null, out ret);
		ret = ret + (rectTransform.rect.size / 2.0f);
		return ret;
	}

	public Vector2 getScreenCoordFromPixel(Vector2 coord) {
		throw new NotImplementedException();
	}

	//Update position if dragging
	public void Update() {

		if (Input.GetMouseButtonDown(2)) {
			DragStart();
		}
		if (Input.GetMouseButtonUp(2)) {
			DragEnd();
		}
		if (Input.mouseScrollDelta.y != 0)
			OnScroll();

		//Update position if dragging
		if (dragging)
			rectTransform.position = Input.mousePosition - this.offset;
	}
}
