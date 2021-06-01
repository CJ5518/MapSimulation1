// Decompiled with JetBrains decompiler
// Type: DraggableButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DADC71AF-6ED1-41B5-9B7D-530B78799929
// Assembly location: C:\Users\carso\Desktop\Build\MapSimulation0_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class DraggableButton : Button {
	private Vector3 offset;
	private bool dragging;

	public override void OnPointerDown(PointerEventData eventData) {
		base.OnPointerDown(eventData);
		this.offset = Input.mousePosition - this.GetComponent<RectTransform>().position;
		this.dragging = true;
	}

	public override void OnPointerUp(PointerEventData eventData) {
		base.OnPointerUp(eventData);
		this.dragging = false;
	}

	public void Update() {
		if (!this.dragging)
			return;
		this.GetComponent<RectTransform>().position = Input.mousePosition - this.offset;
	}
}
