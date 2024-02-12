using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Silly little script that just does title screen stuff
//Lives on 'ButtonsBracket' for some reason idk man I thought I was done with this project
public class TitleScreenButtons : MonoBehaviour {
	int scenarioDesired = 1;
	public GameObject scenarioPanel;
	public GameObject activity2Panel;
	public GameObject activity3Panel;

	//Button on click functions, set in the editor (make sure to) 
	public void scenario1ButtonClick() {
		scenarioDesired = 1;
		scenarioPanel.SetActive(true);
	}
	public void scenario2ButtonClick() {
		scenarioDesired = 2;
		scenarioPanel.SetActive(true);
	}
	public void scenarioPanelNextButtonClicked() {
		scenarioPanel.SetActive(false);
		if (scenarioDesired == 1) {
			activity2Panel.SetActive(true);
		} else {
			activity3Panel.SetActive(true);
		}
	}
	public void scenarioPanelBackButtonClicked() {
		scenarioPanel.SetActive(false);
	}

	//These are the ones that actually set up the simulation
	public void activity2PanelNextButtonClicked() {
		defualtParams();
		SimulationSetupData.enableAirports = false;
		SceneManager.LoadScene("UltraMainScene");
	}
	public void activity3PanelNextButtonClicked() {
		defualtParams();
		SimulationSetupData.enableAirports = true;
		SceneManager.LoadScene("UltraMainScene");
	}

	private void defualtParams() {
		SimulationSetupData.useTheseNumbers = true;
		SimulationSetupData.useStochasticModel = true;
		SimulationSetupData.startImmediatelyPositionIndex = 20331;
		SimulationSetupData.startImmediatelyAtPosition = true;
		SimulationSetupData.movementModel = 1;

		SimulationSetupData.infectionLength = 4.10f;
		SimulationSetupData.recoveryRate = 0.99f;
		SimulationSetupData.waningImmunity = 0;
		SimulationSetupData.latencyEI = 1.9f;
		//(SimulationSetupData.contactRate / 24.0f) * SimulationSetupData.infectionProbability;
		SimulationSetupData.contactRate = 12.0f;
		SimulationSetupData.infectionProbability = 0.1f;

		SimulationSetupData.roadFactor = 3.0f;
		SimulationSetupData.waterFactor = 3.0f;
		SimulationSetupData.heightFactor = 3.0f;
	}

	public void activity2PanelBackButtonClicked() {
		scenarioPanel.SetActive(true);
		activity2Panel.SetActive(false);
	}
	public void activity3PanelBackButtonClicked() {
		scenarioPanel.SetActive(true);
		activity3Panel.SetActive(false);
		
	}
	public void exitButtonClick() {
		Application.Quit();
	}
}
