using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Collections.Generic;

public class ParameterShapeHandler : MonoBehaviour {
	//The main line renderer of the shape
	public UILineRenderer lineRenderer;
	public string[] labels = {
		"α", "γ", "σ", "δ", "β"
	};
	[Header("Please don't mess with these, a for loop depends on this")]
	public ParameterSlider[] parameterSliders;

	//Template game object, for the labels
	public GameObject template;

	void Start() {
		//Set up the line renderer
		lineRenderer.Points = new Vector2[labels.Length + 1];


		//Create the labels
		float angle = 0.0f;
		float deltaAngle = 360.0f / labels.Length;
		for (int q = 0; q < labels.Length; q++) {
			GameObject item = Instantiate(template);
			item.transform.SetParent(template.transform.parent, false);
			RectTransform rectTransform = item.GetComponent<RectTransform>();
			TMP_Text text = item.GetComponentInChildren<TMP_Text>();
			text.text = labels[q];
			rectTransform.transform.localEulerAngles = new Vector3(0, 0, angle);
			text.transform.localEulerAngles = new Vector3(0, 0, -angle);
			item.SetActive(true);
			angle += deltaAngle;
		}
		UpdateShape();
	}

	//Id in range of [0, labels.Length)
	//Val in range [0, 1] or more/less if you really want
	void setLabelValue(int id, float val) {
		float angle = (360.0f / labels.Length) * id;
		float max = template.GetComponent<RectTransform>().rect.height;
		float y = Mathf.Sin(Mathf.Deg2Rad * (angle + 90.0f)) * val * max;
		float x = Mathf.Cos(Mathf.Deg2Rad * (angle + 90.0f)) * val * max;
		lineRenderer.Points[id] = new Vector2(x,y);
	}

	public void UpdateShape() {
		//Depends on the way things are defined in the parameter slider list
		for (int q = 0; q < 4; q++) {
			setLabelValue(q, parameterSliders[q].scaledValue);
		}
		//Set Beta
		setLabelValue(4, parameterSliders[4].scaledValue * parameterSliders[5].scaledValue);
		lineRenderer.Points[lineRenderer.Points.Length - 1] = lineRenderer.Points[0];
		lineRenderer.SetAllDirty();
	}
}

//Rotate on z axis /\ is 0 < is 90 \/ is 180
//Text mesh pro rotation is -z axis rotation