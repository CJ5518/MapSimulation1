using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//Interventions panel class
//In the future I would like to have each intervention as its own class
//But right now things needed to be squished in so this is what you get
public class InterventionPanel : MonoBehaviour {
	public Main main;
	public ParameterSlider vaccPercentSlider;
	public ParameterSlider vaccRolloutTimeSlider;
	public Button applyButton;
	//Gotta know what state this is somehow
	const int vaccinationStateIdx = 5;

	//Things to do
	int pendingVaccTicks = 0;
	float vaccPercentPerTick = 0.0f;
	
	//Outgoing events
	public UnityEvent vaccRolloutStart;
	public UnityEvent vaccRolloutEnd;

	bool firstVaccWave = false;

	void Start() {
		applyButton.onClick.AddListener(apply);
		main.simulation.onTickEnd.AddListener(onTickEnd);
	}
	
	//Apply the misc settings of the intervention panel
	void apply() {
		if (pendingVaccTicks > 0) return;
		pendingVaccTicks = (int)vaccRolloutTimeSlider.value;
		vaccPercentPerTick = vaccPercentSlider.scaledValue /  vaccRolloutTimeSlider.scaledValue;
		vaccPercentSlider.sliderValue = 0;
		firstVaccWave = true;
	}

	//Function called whenever a tick ends, and thus it is safe to write to the simulation
	void onTickEnd() {
		if (pendingVaccTicks <= 0)
			return;
		if (firstVaccWave) {
			firstVaccWave = false;
			vaccRolloutStart.Invoke();
		}
		for (int q = 0; q < main.simulation.width * main.simulation.height; q++) {
			if (!main.simulation.cellIsValid(q))
				continue;
			Simulation.Cell readCell = main.simulation.readCells[q];
			int sus = readCell.state.state[main.simulation.model.startingStateIdx];
			
			//The number of people changing states
			int difference = (int)(sus * vaccPercentPerTick);

			//-Sus
			readCell.state.state[main.simulation.model.startingStateIdx] -= difference;
			//+Vacc
			readCell.state.state[vaccinationStateIdx] += difference;

			//Probably useless to write it back, but better safe than sorry
			main.simulation.readCells[q] = readCell;
		}

		pendingVaccTicks--;

		if (pendingVaccTicks == 0) {
			vaccRolloutEnd.Invoke();
		}

		//RETURN statement above, watch out mate
	}
}
