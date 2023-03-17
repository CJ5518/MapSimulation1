using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class SliderTextCombo : MonoBehaviour {
	public enum DisplayType {
		Default,
		Inverse
	}
	public Slider slider;
	public TMP_Text text;
	public DisplayType displayType = DisplayType.Default;
	public string textPrefix = "r0: ";
	public string textPostfix = "";
	public int significantDigits = 3;
	public float defaultValue = 0.0f;

	void Start() {
    }

	void onSliderValueChangedDefault(float value) {
		text.text = textPrefix + slider.value.ToString("F" + significantDigits) + textPostfix;
	}
	void onSliderValueChangedInverse(float value) {
		if (slider.value > 0.0f)
			onSliderValueChangedDefault(1.0f / slider.value);
		else
			onSliderValueChangedDefault(float.PositiveInfinity);
	}
}
