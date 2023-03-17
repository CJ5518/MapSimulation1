using UnityEngine;
using UnityEngine.UI;

public class QuestionSlider : MonoBehaviour {

	Slider questionSlider; // this script will be put on the slider
	public Button nextButton;
	public Button nextArrowButton;
	public Button backArrowButton;

	void Start() {
		questionSlider = GetComponent<Slider>();
		//nextButton.onClick.AddListener(CompartmentEvents.IncValue);
		//nextArrowButton.onClick.AddListener(Next);
		//backArrowButton.onClick.AddListener(Back);
		CompartmentEvents.OnValueChange += SetValue;
		questionSlider.onValueChanged.AddListener(ListenerWrapper);
	}

	//onValueChanged gives a float and won't auto convert
	private void ListenerWrapper(float value)
    {
		CompartmentEvents.SetNewValue((int)value - 1);

	}

	public void SetValue(int newValue)
    {
		newValue++;
		//Cyclical calling by ListenerWrapper
		if (newValue == questionSlider.value)
		{
			//Debug.LogError("QuestionSlider.SetValue new value same as old");
			return;
		}
		questionSlider.value = newValue;
    }	
}