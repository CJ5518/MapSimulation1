using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChyronManager : MonoBehaviour {
	//The template object for all the elements
	public GameObject chyronElementTemplate;
	//The object that is the parent of all the chyron elements
	public GameObject chyronParentObject;
	//Pretty much need to turn this guy off to have it scroll properly when a new event happens
	public Scrollbar scrollbar;

	//Not so temporary, needed to get the time of an event
	public TempTimeScript tempTimeScript;

	//Objects needed because they emit events

	public InterventionPanel interventionPanel;

	void Start() {
		SimulationManager.stats.infectionReachesState.AddListener(onInfectionReachesStates);
		SimulationManager.stats.infectionDiesOut.AddListener(onInfectionDiesOut);
		scrollbar.onValueChanged.AddListener(onScrollSliderValueChanged);
	}

	void onInfectionDiesOut() {
		createElement($"Infection died out!");
	}
	
	void onInfectionReachesStates(int stateIdx) {
		createElement($"Infection surfaced in {SimulationManager.stats.stateNames[stateIdx]}");
	}

	void createElement(string text) {
		GameObject newElement = GameObject.Instantiate(chyronElementTemplate, chyronParentObject.transform);
		GameObject eventTextObject = newElement.transform.Find("EventText").gameObject;
		eventTextObject.GetComponent<TMP_Text>().text = text;
		newElement.transform.Find("Time").gameObject.GetComponent<TMP_Text>().text = tempTimeScript.GetTimeText();
		newElement.SetActive(true);
		BehaviourLogger.logItem("MadeChyronEntry_" + tempTimeScript.GetTimeText() + "_" + text);
	}

	public void onScrollSliderValueChanged(float value) {
		//Also doesn't let the user scroll but, whatever
		scrollbar.value = 0;
	}
}
