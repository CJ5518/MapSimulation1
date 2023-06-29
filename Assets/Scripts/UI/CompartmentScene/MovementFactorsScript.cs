using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementFactorsScript : MonoBehaviour {
	public Slider waterSlider = null;
	public Slider roadSlider = null;
	public Slider heightSlider = null;

	void OnDisable() {
		if (waterSlider != null) {
			SimulationSetupData.waterFactor = waterSlider.value;
		}
		if (roadSlider != null) {
			SimulationSetupData.roadFactor = roadSlider.value;
		}
		if (heightSlider != null) {
			SimulationSetupData.heightFactor = heightSlider.value;
		}
	}
}