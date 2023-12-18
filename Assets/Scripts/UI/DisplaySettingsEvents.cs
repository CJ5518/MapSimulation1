using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySettingsEvents : MonoBehaviour {
	public GameObject USAMesh;
	public Texture StateLinesTexture;
	public Texture NoStateLinesTexture;
	public void SetStateLinesVisibility(Toggle toggle) {
		SetStateLinesVisibility(toggle.isOn);
	}
	public void SetStateLinesVisibility(bool state) {
		if (state) {
			TurnOnStateLines();
			BehaviourLogger.logItem("StateLineDisplayTurnedOn");
		} else {
			TurnOffStateLines();
			BehaviourLogger.logItem("StateLineDisplayTurnedOff");
		}
	}

	public void SetPlaneTrailsActive(Toggle toggle) {
		SetPlaneTrailsActive(toggle.isOn);
	}
	public void SetPlaneTrailsActive(bool state) {
		SimulationManager.simulation.simulationAirports.drawPlaneTrails = state;
		if (state) {
			BehaviourLogger.logItem("PlaneTrailDisplayTurnedOn");
		} else {
			BehaviourLogger.logItem("PlaneTrailDisplayTurnedOff");
		}
	}
	private void TurnOffStateLines() {
		USAMesh.GetComponent<MeshRenderer>().material.SetTexture("_Details", NoStateLinesTexture);
	}

	private void TurnOnStateLines() {
		USAMesh.GetComponent<MeshRenderer>().material.SetTexture("_Details", StateLinesTexture);
	}
}
