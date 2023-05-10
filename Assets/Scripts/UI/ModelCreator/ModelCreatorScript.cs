using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ModelCreatorScript : MonoBehaviour {
	string mode = "compartment";
	string newName = "Susceptible";
	int selectedIdx = -1;

	int[,] stoichiometry;

	struct Arrow {
		public int idxTo;
		public int idxFrom;
		public bool grey;
	}

	struct Compartment {
		public Vector2 position;
		public string name;
	}

	List<Compartment> compartments;
	List<Arrow> arrows;
	

	void Start() {
		compartments = new List<Compartment>();
		arrows = new List<Arrow>();
		Application.targetFrameRate = 60;
	}
	
	void Update() {
		//Change states
		if (Input.GetKeyDown(KeyCode.Return)) {
			if (mode == "compartment") {
				mode = "blackLines";
			} else if (mode == "blackLines") {
				mode = "greyLines";
			} else if (mode == "greyLines") {
				onModelFinalized();
			}
		}

		//Events occur
		if (mode == "compartment" && Input.GetMouseButtonDown(0) && (Input.mousePosition.x > 100 || Input.mousePosition.y < Screen.height - 200)) {
			Compartment newCompartment = new Compartment();
			newCompartment.name = newName;
			newCompartment.position = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			compartments.Add(newCompartment);
		}
	}

	void onModelFinalized() {
		//Generate the stoichiometry
		stoichiometry = new int[arrows.Count, compartments.Count];
		for (int q = 0; q < arrows.Count; q++) {
			for (int j = 0; j < compartments.Count; j++) {
				stoichiometry[q,j] = 0;
			}
			stoichiometry[q, arrows[q].idxFrom] = -1;
			stoichiometry[q, arrows[q].idxTo] = 1;
		}
		string propensityFuncs = "";
		//Generate the propensity funcs
		for (int q = 0; q < arrows.Count; q++) {
			if (arrows[q].grey) {
				propensityFuncs += $"ret.propensityDetails[{q}] = new int[4] {{1,{arrows[q].idxFrom},{arrows[q].idxTo},{q}}};\n";
			} else {
				propensityFuncs += $"ret.propensityDetails[{q}] = new int[3] {{0,{arrows[q].idxFrom},{q}}};\n";
			}
		}
		Logger.Log(propensityFuncs);
		string stoichString = "";
		//Stringify the stoichiometry
		//It was easier in my head to first put it in the array and then stringify it
		for (int x = 0; x < stoichiometry.GetLength(0); x++) {
			for (int y = 0; y < stoichiometry.GetLength(1); y++) {
				stoichString += $"ret.stoichiometry[{x},{y}] = {stoichiometry[x,y]};\n";
			}
		}
		Logger.Log(stoichString);
	}

	void OnGUI() {
		GUI.Label(new Rect(0,0,100,100), mode);
		newName = GUI.TextField(new Rect(0,100, 100, 100), newName);
		for (int q = 0; q < arrows.Count; q++) {
			Vector2 from = compartments[arrows[q].idxFrom].position;
			Vector2 to = compartments[arrows[q].idxTo].position;
			
			float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
		
			GUIUtility.RotateAroundPivot(angle, compartments[arrows[q].idxFrom].position);
			GUI.Button(new Rect(from, new Vector2((from - to).magnitude, 20)), q.ToString());
			GUIUtility.RotateAroundPivot(-angle, compartments[arrows[q].idxFrom].position);
		}
		for (int q = 0; q < compartments.Count; q++) {
			if (GUI.Button(new Rect(compartments[q].position, new Vector2(100,100)), compartments[q].name)) {
				if (mode.Contains("Lines")) {
					if (selectedIdx >= 0) {
						Arrow newArrow = new Arrow();
						newArrow.idxFrom = selectedIdx;
						newArrow.idxTo = q;
						newArrow.grey = mode == "greyLines";
						arrows.Add(newArrow);
						selectedIdx = -1;
					} else {
						selectedIdx = q;
					}
				}
			}
		}
	}
}
