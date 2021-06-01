// Decompiled with JetBrains decompiler
// Type: MovableRawImage
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DADC71AF-6ED1-41B5-9B7D-530B78799929
// Assembly location: C:\Users\carso\Desktop\Build\MapSimulation0_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MovableRawImage :
  RawImage,
  IScrollHandler,
  IEventSystemHandler,
  IPointerDownHandler,
  IPointerUpHandler {
	private Vector3 offset;
	private bool dragging;

	public void OnPointerDown(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Middle)
			return;
		this.offset = Input.mousePosition - this.GetComponent<RectTransform>().position;
		this.dragging = true;
	}

	public void OnPointerUp(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Middle)
			return;
		this.dragging = false;
	}

	public void OnScroll(PointerEventData eventData) {
		float y = Input.mouseScrollDelta.y;
		RectTransform component = this.GetComponent<RectTransform>();
		float num = (float)(0.0500000007450581 * (double)y + 1.0);
		Vector2 localPoint1;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(component, (Vector2)Input.mousePosition, (Camera)null, out localPoint1);
		RectTransform rectTransform1 = component;
		rectTransform1.localScale = rectTransform1.localScale * num;
		Vector2 localPoint2;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(component, (Vector2)Input.mousePosition, (Camera)null, out localPoint2);
		Vector2 vector2 = localPoint2 - localPoint1;
		RectTransform rectTransform2 = component;
		rectTransform2.position = rectTransform2.position + new Vector3(vector2.x, vector2.y) * component.localScale.x;
	}

	public Vector2 getPixelFromScreenCoord(Vector2 coord) {
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, coord, (Camera)null, out localPoint);
		Vector2 vector2 = localPoint + this.rectTransform.rect.size / 2f;
		vector2.x /= this.rectTransform.rect.width;
		vector2.y /= this.rectTransform.rect.height;
		vector2.x *= (float)this.texture.width;
		vector2.y *= (float)this.texture.height;
		return vector2;
	}

	public Vector2 getScreenCoordFromPixel(Vector2 coord) => throw new NotImplementedException();

	public void Update() {
		if (!this.dragging)
			return;
		this.GetComponent<RectTransform>().position = Input.mousePosition - this.offset;
	}
}
