using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SliderTextCombo : MonoBehaviour {
	public enum DisplayType {
		Default,
		Inverse
	}
	public Slider slider;
	public Text text;
	public DisplayType displayType = DisplayType.Default;
	public string textPrefix = "r0: ";
	public string textPostfix = "";
	public int significantDigits = 3;
	public float defaultValue = 0.1f;

	void Start() {
		//Set the callback based on the DisplayType
		switch (displayType) {
			case DisplayType.Default:
			slider.onValueChanged.AddListener(onSliderValueChanged);
			break;

			case DisplayType.Inverse:
			slider.onValueChanged.AddListener(onSliderValueChangedInverse);
			break;
		}
		slider.value = defaultValue;
	}

	void onSliderValueChanged(float value) {
		text.text = textPrefix + value.ToString("F" + significantDigits) + textPostfix;
	}
	void onSliderValueChangedInverse(float value) {
		if (value > 0.0f)
			onSliderValueChanged(1.0f / value);
		else
			onSliderValueChanged(0.0f);
	}
}
