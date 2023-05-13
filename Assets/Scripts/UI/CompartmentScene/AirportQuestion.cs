using UnityEngine;
using UnityEngine.UI;

//Didn't feel like dealing with the rest of the thing so I just hacked this question in
public class AirportQuestion : MonoBehaviour {
	public Toggle enableToggle;

	void OnDisable() {
		SimulationSetupData.enableAirports = enableToggle.isOn;
	}
}
