using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickyCursorButton : Button {
	public override void OnPointerEnter(PointerEventData eventData) {
		CursorHelper.setClickyCursor();
	}
	public override void OnPointerExit(PointerEventData eventData) {
		CursorHelper.resetCursor();
	}
}