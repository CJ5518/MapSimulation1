using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Manages the colors of the simulation and creates a little interface for them too
public class ColorSettingsPanel : MonoBehaviour {
	public GameObject templateObject;
	public GameObject parentObject;
	[Header("The thingy with the material on it")]
	public MeshRenderer shaderMeshRenderer;
	private GameObject[] toggles;
	const int maxTogglesOn = 4;

	public void loadInSettings(ref SimulationModel model) {
		toggles = new GameObject[model.compartmentCount];
		//Clone the thingies
		for (int q = 0; q < model.compartmentCount; q++) {
			GameObject newObject = GameObject.Instantiate(templateObject, parentObject.transform);
			GameObject colorButtonObject = newObject.transform.Find("ColorButton").gameObject;
			GameObject labelObject = newObject.transform.Find("Label").gameObject;
			colorButtonObject.GetComponent<Image>().color = model.compartmentInfoArray[q].mapDisplayColor;
			labelObject.GetComponent<Text>().text = model.compartmentInfoArray[q].longName;
			newObject.SetActive(true);
			toggles[q] = newObject;
		}
		//Temp thing to make the display settings what we want for the demo
		setToggleState(1, true);
		setToggleState(0, false);
		setToggleState(3, false);
		setToggleState(4, true);
		onSettingsChanged();
	}

	//Checks if the toggle 
	private bool isToggleOn(int idx) {
		return toggles[idx].GetComponent<Toggle>().isOn;
	}

	private Color getToggleColor(int idx) {
		return toggles[idx].transform.Find("ColorButton").gameObject.GetComponent<Image>().color;
	}

	private void setToggleState(int idx, bool state) {
		toggles[idx].GetComponent<Toggle>().isOn = state;
	}


	bool thingsHaveChanged = true;
	//Update the simulations colors
	public void setSimulationColors(ref Simulation simulation) {
		if (!thingsHaveChanged) return;
		thingsHaveChanged = false;

		
		simulation.statesToDraw.Clear();
		int colorCount = 1;
		for (int q = 0; q < toggles.Length; q++) {
			
			
			if (isToggleOn(q)) {
				//Adjusting the simulation states to draw
				simulation.statesToDraw.Add(q);

				//Adjusting the colors of the shader
				shaderMeshRenderer.material.SetColor($"_Color{colorCount}", getToggleColor(q));
				colorCount++;
			}
		}
	}

	//Called by the toggle's events
	public void onSettingsChanged() {
		thingsHaveChanged = true;
		int onCount = 0;
		string onBits = "";
		for (int q = 0; q < toggles.Length; q++) {
			if (isToggleOn(q)) onCount++;
			if (onCount > maxTogglesOn) {
				setToggleState(q, false);
			}
			onBits += isToggleOn(q) ? "1" : "0";
		}
		BehaviourLogger.logItem("DisplayColorsChanged_NewState=" + onBits);
	}
}
