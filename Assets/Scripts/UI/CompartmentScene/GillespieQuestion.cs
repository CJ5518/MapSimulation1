using UnityEngine;
using UnityEngine.UI;

//Didn't feel like dealing with the rest of the thing so I just hacked this question in
public class GillespieQuestion : MonoBehaviour {
	public Toggle deterministicToggle;

	void OnDisable() {
		SimulationSetupData.useStochasticModel = !deterministicToggle.isOn;
	}
}
