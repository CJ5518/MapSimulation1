using UnityEngine;

public class TogglePopulationDisplay : MonoBehaviour{

	Material mat;
	Color susceptibleColor;
	bool displaySusceptible = true;

	void Start() {
		mat = GetComponent<MeshRenderer>().material;
		susceptibleColor = mat.GetColor("_Vaccinated");
    }

	public void ToggleSusceptible() {
		Color c = Color.black;
		if (!displaySusceptible) c = susceptibleColor;
		mat.SetColor("_Vaccinated", c);
		displaySusceptible = !displaySusceptible;
	}
}