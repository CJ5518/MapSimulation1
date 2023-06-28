using UnityEngine;

//https://www.nature.com/articles/nature10856
//Also hey check out this paper: https://www.pnas.org/doi/full/10.1073/pnas.0906910106
public class LocalizedGravityMovementModel : SimulationMovementModel {
	//param[0] = alpha
	//param[1] = beta
	public float[] parameters = new float[2];
	ParameterSliderSettings[] sliderSettings;

	public LocalizedGravityMovementModel(float alpha, float beta) {
		sliderSettings = new ParameterSliderSettings[2];
		sliderSettings[0].displayType = ParameterSlider.DisplayType.Default;
		sliderSettings[0].maxValue = 2.0f;
		sliderSettings[0].minValue = 0.0f;
		sliderSettings[0].scaleType = ParameterSlider.ScaleType.None;
		sliderSettings[0].startingValue = alpha;
		sliderSettings[0].textPrefix = "Alpha: ";

		sliderSettings[1].displayType = ParameterSlider.DisplayType.Default;
		sliderSettings[1].maxValue = 2.0f;
		sliderSettings[1].minValue = 0.0f;
		sliderSettings[1].scaleType = ParameterSlider.ScaleType.None;
		sliderSettings[1].startingValue = beta;
		sliderSettings[1].textPrefix = "Beta: ";

		parameters[0] = alpha;
		parameters[1] = beta;
	}


	public override float getCellSpreadValue(int idxGiver, int idxReceiver, Simulation simulation) {
		Simulation.Cell giverCell = simulation.readCells[idxGiver];
		Simulation.Cell receiverCell = simulation.readCells[idxReceiver];
		Vector2Int distanceVec = simulation.indexToCoord(idxGiver) - simulation.indexToCoord(idxReceiver);
		//If the neighbors are diagonal, then the distance is the sqrt of two, otherwise it is 1
		float distance = Mathf.Abs(distanceVec.x) + Mathf.Abs(distanceVec.y) == 2 ? 1.414f : 1;

		//We divide by 121 because uhh... um... it's because of the...
		//Yea idk
		float amount = (
			Mathf.Pow(giverCell.state.state[simulation.model.droppingStateIdx], parameters[0])
		* Mathf.Pow(receiverCell.state.state[simulation.model.startingStateIdx], parameters[1])
		) / (distance * 121.0f);

		amount = doBasicDataLayers(amount, ref receiverCell, ref giverCell);

		//Lame fix for the bug where a cell would give too many things
		//As well as the negative people bug
		amount = Mathf.Clamp(amount, 0.0f, giverCell.state.state[simulation.model.droppingStateIdx] / 15.0f);

		return amount;
	}

	public override ParameterSliderSettings[] getSliderSettings(bool duringSimulation) {
		return sliderSettings;
	}

	//Update the movement model parameters given the values
	//The values are given in the same order they received from getSlierNames, which is why we need
	//the duringSimulation bool passed through here as well
	public override void updateSliderValues(float[] values, bool duringSimulation) {
		parameters[0] = values[0];
		parameters[1] = values[1];
	}
}
