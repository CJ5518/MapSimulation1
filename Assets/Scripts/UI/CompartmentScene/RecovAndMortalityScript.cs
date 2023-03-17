using UnityEngine;
using UnityEngine.UI;

//Hacky thing because a demo needed to be made
public class RecovAndMortalityScript : MonoBehaviour {
	public Slider recovSlider;
	public Slider deathSlider;

	void Start() {
		deathSlider.value = 1;
	}

	//Dumb hack to get the slider to have the right bloody starting value
	bool hasHappened = false;
	void Update() {
		
	}
}
