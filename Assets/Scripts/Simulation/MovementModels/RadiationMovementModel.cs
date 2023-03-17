using UnityEngine;
using System.Threading;
using System.IO;

//Based on:
//https://doi.org/10.1038/nature10856
public class RadiationMovementModel : SimulationMovementModel {
	float[][] movementMatrix;
	//Defined at 0.11f for the USA
	//I guess
	//The proportion of the population which commutes
	//I suppose this could be a very granulated data point, the proportion of each cell commutes differently, no?
	//But I like this model because we don't need any data other than this, and it has just this one number which I really like
	float commutingRate = 0.11f;

	
	public override float getCellSpreadValue(int idxGiver, int idxReceiver, Simulation simulation) {
		Simulation.Cell giverCell = simulation.readCells[idxGiver];
		Simulation.Cell receiverCell = simulation.readCells[idxReceiver];

		return getCommuters(idxGiver, idxReceiver, simulation) / 24.0f;
	}

	public override ParameterSliderSettings[] getSliderSettings(bool duringSimulation) {
		return new ParameterSliderSettings[]{};
	}

	//Update the movement model parameters given the values
	//The values are given in the same order they received from getSlierNames, which is why we need
	//the duringSimulation bool passed through here as well
	public override void updateSliderValues(float[] values, bool duringSimulation) {

	}

	//Get the average commuters from I -> J, not sure what timescale this is yet, we'll figure it out
	private float getCommuters(int idxI, int idxJ, Simulation simulation) {
		float mi = simulation.readCells[idxI].state.state[simulation.model.droppingStateIdx];
		float nj = simulation.readCells[idxJ].state.numberOfPeople;
		float Ti = commutingRate * mi;
		float sij = calculateCirclePopulation(idxI, idxJ, simulation);
		float denominator = ((mi + sij) + (mi + nj + sij));
		return denominator > 0.0f ? (Ti * nj * mi) / denominator : 0.0f;
	}



	//Calculate the total population in the circle of radius of distance from center to dest centered at center
	//Does not include the population of the center or destination cells
	private float calculateCirclePopulation(int idxCenter, int idxDestination, Simulation simulation) {
		float countedPop = 0.0f;

		//We do not do calculations with any amount of distance beyond the immediate 8 neighbors now
		int[] neighbors = simulation.getNeighborIndices(idxCenter);
		for (int q = 0; q < neighbors.Length; q++) {
			if (simulation.cellIsValid(neighbors[q]) && idxDestination != neighbors[q]) {
				countedPop += simulation.readCells[neighbors[q]].state.numberOfPeople;
			}
		}
		return countedPop;
	}
}
