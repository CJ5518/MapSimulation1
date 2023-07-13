using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public struct ParameterSliderSettings {
	public ParameterSlider.ScaleType scaleType;
	public ParameterSlider.DisplayType displayType;
	public float minValue;
	public float maxValue;
	public float startingValue;
	public string textPrefix;
}


//Handles the parameter panel
//Call load in sliders with your model to load in the correct sliders
//Then you can call update parameters to update the parameters
public class ParameterPanel : MonoBehaviour {
	public Main main;
	public GameObject sliderParent;
	public GameObject sliderTemplate;

	public List<ParameterSlider> parameterSliders;
	public List<ParameterSlider> movementModelSliders;

	public void loadInSliders(ref SimulationModel model, ref SimulationMovementModel movementModel) {
		parameterSliders = new List<ParameterSlider>();
		for (int q = 0; q < model.parameterCount; q++) {
			ParameterSliderSettings settings = new ParameterSliderSettings();
			settings.startingValue = model.parameters[q];
			settings.textPrefix = model.parameterInfoArray[q].longName + ": ";
			settings.minValue = 0;
			settings.maxValue = 1.0f;
			settings.scaleType = ParameterSlider.ScaleType.None;
			parameterSliders.Add(generateSlider(settings));
		}

		movementModelSliders = new List<ParameterSlider>();
		ParameterSliderSettings[] sliderSettings = movementModel.getSliderSettings(true);
		for (int q = 0; q < sliderSettings.Length; q++) {
			movementModelSliders.Add(generateSlider(sliderSettings[q]));
		}
	}

	private ParameterSlider generateSlider(ParameterSliderSettings settings) {
		sliderTemplate.GetComponent<Slider>().value = settings.startingValue;
		GameObject newSlider = Instantiate(sliderTemplate, sliderParent.transform);
		ParameterSlider sliderPart = newSlider.GetComponent<ParameterSlider>();
		sliderPart.displayType = settings.displayType;
		sliderPart.scaleType = settings.scaleType;
		newSlider.SetActive(true);
		sliderPart.textPrefix = settings.textPrefix;
		sliderPart.minValue = settings.minValue;
		sliderPart.maxValue = settings.maxValue;
		sliderPart.value = settings.startingValue;
		return sliderPart;
	}

	public void updateParameters(ref SimulationModel model, ref SimulationMovementModel movementModel) {
		for (int q = 0; q < parameterSliders.Count; q++) {
			model.parameters[q] = parameterSliders[q].scaledValue;
			//I'm not too sure what this is or does but ok
			parameterSliders[q].GraphicUpdateComplete();
		}
		float[] values = new float[movementModelSliders.Count];
		for (int q = 0; q < movementModelSliders.Count; q++) {
			values[q] = movementModelSliders[q].scaledValue;
			movementModelSliders[q].GraphicUpdateComplete();
		}
		movementModel.updateSliderValues(values, true);
	}
}
