using UnityEngine;
using UnityEngine.UI;
using TMPro;
[System.Serializable]
public class ParameterSlider : Slider {
	public enum DisplayType {
		Default,
		Inverse,
		Percentage
	}
	public enum ScaleType {
		Squared,
		None
	}
	
	public TMP_Text text;
	public DisplayType displayType = DisplayType.Default;
	public ScaleType scaleType = ScaleType.None;
	public string textPrefix = "r0: ";
	public string textPostfix = "";
	public int significantDigits = 3;
	public float defaultSliderValue = 0.0f;


	//From min to max
	public float sliderValue {
		get {
			return m_Value;
		}
		set {
			m_Value = value;
		}
	}

	//The scaled value of the slider
	public float scaledValue {
		get {
			switch (scaleType) {
				case ScaleType.Squared:
				return m_Value * m_Value;

				case ScaleType.None:
				return m_Value;
			}
			Debug.LogError("Scale type not accounted for!");
			throw new System.NotImplementedException();
		}
	}

	protected override void Start() {
		base.Start();

		//Set the callback based on the DisplayType
		switch (displayType) {
			case DisplayType.Default:
			onValueChanged.AddListener(onSliderValueChangedDefault);
			onSliderValueChangedDefault(defaultSliderValue);
			break;

			case DisplayType.Inverse:
			onValueChanged.AddListener(onSliderValueChangedInverse);
			onSliderValueChangedInverse(defaultSliderValue);
			break;

			case DisplayType.Percentage:
			onValueChanged.AddListener(onSliderValueChangedPercentage);
			onSliderValueChangedPercentage(defaultSliderValue);
			break;
		}
	}

	//Event handlers for the different display types
	void onSliderValueChangedDefault(float value) {
		setTextToNum(scaledValue);
	}
	void onSliderValueChangedInverse(float value) {
		if (scaledValue > 0.0f)
			setTextToNum(1.0f / scaledValue);
		else
			setTextToNum(float.PositiveInfinity);
	}
	void onSliderValueChangedPercentage(float value) {
		setTextToNum(scaledValue * 100);
	}

	//Most of them just transform a number before drawing it in this way
	void setTextToNum(float num) {
		text.text = textPrefix + num.ToString("F" + significantDigits) + textPostfix;
	}
}