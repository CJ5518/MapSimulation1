using UnityEngine;

public class CJsMovementModel : SimulationMovementModel {

	//Fudge factor
	float spreadRate = 1.0f;

	public override float getCellSpreadValue(int idxGiver, int idxReceiver, Simulation simulation) {
		Simulation.Cell giverCell = simulation.readCells[idxGiver];
		Simulation.Cell receiverCell = simulation.readCells[idxReceiver];
		float amount = giverCell.state.state[simulation.model.droppingStateIdx] / 80.0f;

		
		//Population fudge factor
		float populationFactor = Mathf.Clamp(Mathf.Sqrt(giverCell.state.numberOfPeople), float.Epsilon, float.MaxValue)
			/ Mathf.Sqrt(simulation.maxNumberOfPeople);

		amount *= populationFactor * spreadRate;

		amount = doBasicDataLayers(amount, ref receiverCell, ref giverCell);

		//Lame fix for the bug where a cell would give too many things
		if (amount > giverCell.state.state[simulation.model.droppingStateIdx] / 15.0f) amount = giverCell.state.state[simulation.model.droppingStateIdx] / 15.0f;
		//Make sure it's greater than one * dt on our way out
		return amount;
	}

	public override ParameterSliderSettings[] getSliderSettings(bool duringSimulation) {
		ParameterSliderSettings[] settingsArray = new ParameterSliderSettings[1];
		settingsArray[0] = new ParameterSliderSettings();
		settingsArray[0].displayType = ParameterSlider.DisplayType.Default;
		settingsArray[0].scaleType = ParameterSlider.ScaleType.None;
		settingsArray[0].minValue = 0;
		settingsArray[0].maxValue = 2;
		settingsArray[0].startingValue = spreadRate;
		settingsArray[0].textPrefix = "Spread Rate:";
		return settingsArray;
	}

	//Update the movement model parameters given the values
	//The values are given in the same order they received from getSliderSettings, which is why we need
	//the duringSimulation bool passed through here as well
	public override void updateSliderValues(float[] values, bool duringSimulation) {
		spreadRate = values[0];
	}
}
