using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SliderTextCombo : MonoBehaviour {
    public Slider slider;
    public Text text;
    public string textPrefix = "r0: ";
    public int significantDigits = 3;
    public float defaultValue = 0.1f;

    void Start() {
        slider.onValueChanged.AddListener(onSliderValueChanged);
        slider.value = defaultValue;
    }

    void onSliderValueChanged(float value) {
        text.text = textPrefix + slider.value.ToString("F" + significantDigits);
    }
}
