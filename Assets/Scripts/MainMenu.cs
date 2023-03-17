using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	void Start() {
		
	}

	void Update() {
		
	}

	void OnGUI() {
		if (GUI.Button(new Rect(0,0,100,100), "start")) {
			SceneManager.LoadScene("SampleScene");
		}
	}
}
