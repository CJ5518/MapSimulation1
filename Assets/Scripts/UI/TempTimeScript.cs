using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//This class isn't as temporary as I wanted it to be anymore
//Not nearly as temp as I thought it might be
//Nowhere near as tmp as I initially anticipated
public class TempTimeScript : MonoBehaviour {
	public TMPro.TMP_Text text;
	public Button pauseButton;
	public Button playButton;
	public Button fastButton;
	
	void Start() {
		pauseButton.onClick.AddListener(pauseButtonClick);
		playButton.onClick.AddListener(playButtonClick);
		fastButton.onClick.AddListener(fastButtonClick);

		SimulationManager.simulation.onTickEnd.AddListener(updateText);
	}

	void updateText() {
		text.text = GetTimeText();
	}

	public string GetTimeText() {
		//Starting time
		float time = (SimulationManager.simulation.dtSimulated - 1) * 24; //the times 24 is part of outrageous hacks I am doing now 
		//Number of days/weeks passed
		int days = (((int)time) / 24);
		int weeks = days / 7;

		//Strings for days and weeks
		string weekString = (weeks) < 10 ? "0" + weeks.ToString() : weeks.ToString();
		string dayString = "0" + (days % 7).ToString();

		//Dumb ass if statement
		if (time % 24 >= 10) {
			return System.String.Format("{2}:{0}:{1}", dayString, (int)time % 24, weekString);
		}
		else {
			return System.String.Format("{2}:{0}:0{1}", dayString, (int)time % 24, weekString);
		}
	}

	void pauseButtonClick() {
		BehaviourLogger.logItem("PauseButtonClick");
		SimulationManager.TargetTps = 0.0f;
	}

	void playButtonClick() {
		BehaviourLogger.logItem("PlayButtonClick");
		SimulationManager.TargetTps = 5.0f;
	}

	void fastButtonClick() {
		BehaviourLogger.logItem("FastSpeedButtonClick");
		SimulationManager.TargetTps = 100.0f;
	}
}
