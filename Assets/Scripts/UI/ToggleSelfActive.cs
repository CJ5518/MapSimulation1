using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSelfActive : MonoBehaviour {
	public string behaviourLoggerMessage = "";
	public void Toggle() {
		if (behaviourLoggerMessage != "") {
			if (gameObject.activeInHierarchy) {
				BehaviourLogger.logItem(behaviourLoggerMessage + "Deactivated");
			} else {
				BehaviourLogger.logItem(behaviourLoggerMessage + "Activated");
			}
		}
		gameObject.SetActive(!gameObject.activeInHierarchy);
	}
}
