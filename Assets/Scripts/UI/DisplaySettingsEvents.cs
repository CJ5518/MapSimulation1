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
		if (state)
			TurnOnStateLines();
		else
			TurnOffStateLines();

	}
	private void TurnOffStateLines() {
		USAMesh.GetComponent<MeshRenderer>().material.SetTexture("_Details", NoStateLinesTexture);
	}

	private void TurnOnStateLines() {
		USAMesh.GetComponent<MeshRenderer>().material.SetTexture("_Details", StateLinesTexture);
	}
}
