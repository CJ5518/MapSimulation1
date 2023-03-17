using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

public class ParameterHelpButton : ClickyCursorButton {
	public int id;
	public GameObject helpPanel;

	public override void OnPointerClick(PointerEventData eventData) {
		//DISABLE
		return;
		//Collect text components and set the text accordingly
		TMP_Text title = helpPanel.transform.Find("Title").GetComponent<TMP_Text>();
		TMP_Text body = helpPanel.transform.Find("Body").GetComponent<TMP_Text>();
		title.text = "Le bon titre";
		body.text = "Le corps";
		//Position the panel
		RectTransform rectTransform = helpPanel.GetComponent<RectTransform>();
		Bounds buttonBounds = Projection.GetRectTransformBounds(GetComponent<RectTransform>());
		rectTransform.position = buttonBounds.center;
		helpPanel.SetActive(true);
	}
}