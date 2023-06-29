using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementModelParamsQuestion : MonoBehaviour {
	public Slider alphaSlider;
	public Slider betaSlider;
	public Slider spreadRateSlider;
	
	void OnDisable() {
		SimulationSetupData.alpha = alphaSlider.value;
		SimulationSetupData.beta = betaSlider.value;
		SimulationSetupData.spreadRate = spreadRateSlider.value;
	}
}
