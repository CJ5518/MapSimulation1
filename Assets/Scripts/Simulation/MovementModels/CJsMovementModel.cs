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

		//Elevation
		float elevationDiff = receiverCell.elevation - giverCell.elevation;
		float elevationFactor = 1.0f - (elevationDiff / 250.0f);
		//If less than 0, be 0 instead
		elevationFactor = elevationFactor > 0.0f ? elevationFactor : 0.0f;
		//Set to 0 if zombiesCanClimbMountains is false
		elevationFactor = simulation.zombiesCanClimbMountains ? elevationFactor : 0.0f;

		//Roads
		float roadFactor = receiverCell.roadPercent >= 0.4f ? receiverCell.roadPercent * simulation.roadMultiplier : 1.0f;

		//Water
		float waterFactor = simulation.waterAffectsZombies ? (1.0f - receiverCell.waterLevel) : 0.3f;

		//Multiply it by 2 because the misc factors generally go to 1
		amount *= spreadRate * elevationFactor * populationFactor * 2.0f * simulation.dt * waterFactor * roadFactor;
		//Lame fix for the bug where a cell would give too many things
		if (amount > giverCell.state.state[simulation.model.droppingStateIdx] / 15.0f) amount = giverCell.state.state[simulation.model.droppingStateIdx] / 15.0f;
		//Make sure it's greater than one * dt on our way out
		return amount >= 1.0f * simulation.dt ? amount : 0.0f;
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
